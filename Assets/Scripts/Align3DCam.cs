using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Align3DCam : MonoBehaviour
{
    [Tooltip("The transforms the camera attempts to align to.")]
    public Transform tA, tB;

    [SerializeField(), Tooltip("The cinemachine camera that will be updated.")]
    private Cinemachine.CinemachineVirtualCamera virtualCamera;

    /// <summary>
    /// The Transposer component of the cinemachine camera.
    /// </summary>
    private Cinemachine.CinemachineTransposer tranposer;

    /// <summary>
    /// Boolean that is set based on whether or not a virtual camera is supplied.
    /// </summary>
    private bool hasVirtualCamera;

    [SerializeField(), Tooltip("The starting normal of the cinemachine transposer.")]
    private Vector3 framingNormal;

    [SerializeField(), Tooltip("The current distance between the two tracked transforms.")]
    float distance;

    [Tooltip("Slope Value (m) of the linear equation used to determine how far the camera should be based on the distance of the tracked transforms.")]
    public float transposerLinearSlope;

    [Tooltip("Offset Value (b) of the linear equation used to determine how far the camera should be based on the distance of the tracked transforms.")]
    public float transposerLinearOffset;

    [Header("Framing helpers")]
    [Tooltip("The minimum distance allowed between the two transforms before the camera stops moving in and out.")]
    public float minDistance;

    [Tooltip("The minimum distance the camera will be from the tracked transforms.")]
    public float minCamDist;

    [Tooltip("A secondary distance between the two transforms used for reference.")]
    public float secondaryDistance;

    [Tooltip("A secondary distance the camera should be at when the tracked transforms are at the secondary distance.")]
    public float secondaryCamDistance;

    /// <summary>
    /// Function to help determine the
    /// </summary>
    [ContextMenu("Calculate Slope")]
    void CalculateSlopes()
    {
        if (virtualCamera == null)
            return;
        tranposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (tranposer == null)
            return;

        // If the application is playing, we don't update the minimum values.
        if (!Application.isPlaying)
        {
            // We get the distance between the transforms currently
            minDistance = Vector3.Distance(tA.position, tB.position);
            distance = minDistance;

            // We get the magnitude of the follow offset vector.
            minCamDist = tranposer.m_FollowOffset.magnitude;
        }

        // We calculate the slope ((y2-y1)/(x2-x1))
        transposerLinearSlope = (secondaryCamDistance - minCamDist) / (secondaryDistance - minDistance);

        // We calculate the offset b = y - mx;
        transposerLinearOffset = minCamDist - (transposerLinearSlope * minDistance);
    }

    private void Awake()
    {
        // Determines if a virtual camera is present and active.
        hasVirtualCamera = virtualCamera != null;
        if (hasVirtualCamera)
        {
            tranposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            if (tranposer == null)
            {
                hasVirtualCamera = false;
            }
            else
            {
                // Sets the framing normal by the transposer's initial offset.
                framingNormal = tranposer.m_FollowOffset;
                framingNormal.Normalize();
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Gets the distance between the two tracked transforms.
        Vector3 diff = tA.position - tB.position;
        distance = diff.magnitude;

        // The Y is removed and the vector is normalized.
        diff.y = 0f;
        diff.Normalize();

        // Adjusts the follow offset of the transposer based on the distance between the two tracked transforms, using a minimum value.
        if (hasVirtualCamera)
        {
            tranposer.m_FollowOffset = framingNormal * (Mathf.Max(minDistance, distance) *
                transposerLinearSlope + transposerLinearOffset);
        }

        // If the two transforms are at the same position, we don't do any updating.
        if (Mathf.Approximately(0f, diff.sqrMagnitude))
            return;

        // We create a quaternion that looks in the initial direction and rotate it 90 degrees
        Quaternion q = Quaternion.LookRotation(diff, Vector3.up) * Quaternion.Euler(0, 90, 0);

        // We create a second one that is rotated 180 degrees.
        Quaternion qA = q * Quaternion.Euler(0, 180, 0);

        // We determine the angle between the current rotation and the two previously created rotations.
        float angle = Quaternion.Angle(q, transform.rotation);
        float angleA = Quaternion.Angle(qA, transform.rotation);

        // The transform's rotation is set to whichever one is closer to the current rotation.
        if (angle < angleA)
            transform.rotation = q;
        else
            transform.rotation = qA;
    }
}