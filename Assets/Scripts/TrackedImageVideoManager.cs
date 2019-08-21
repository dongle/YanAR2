using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Video;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays a video. Manages the videoplayer and the plane the video
/// is displayed on
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]

public class TrackedImageVideoManager : MonoBehaviour
{
  ARTrackedImageManager m_TrackedImageManager;

  void Awake()
  {
    m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
  }

  void OnEnable()
  {
    m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
  }

  void OnDisable()
  {
    m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
  }

  void UpdateVideo(ARTrackedImage trackedImage)
  {
    if (trackedImage.trackingState == TrackingState.Tracking)
    {
      // The image extents is only valid when the image is being tracked
      trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

      var trackedFrameString = trackedImage.referenceImage.name;
      int trackedFrame = System.Int32.Parse(trackedFrameString.Substring(startIndex: trackedFrameString.Length - 2));

      var videoPlaneParentGO = trackedImage.transform.GetChild(0).gameObject;
      var videoPlaneGO = videoPlaneParentGO.transform.GetChild(0).gameObject;

      if (!videoPlaneGO.activeSelf)
      {
        Debug.Log("started tracking frame " + trackedFrame);
        videoPlaneGO.SetActive(true);
        var videoPlayer = videoPlaneGO.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
          VideoClip clip = Resources.Load<VideoClip>(trackedFrameString) as VideoClip;
          // videoPlayer.clip = clip;
          videoPlayer.Play();
          Debug.Log("started video");
        }
      }
    }
    else
    {
      StopVideo(trackedImage);
    }
  }

  void StopVideo(ARTrackedImage trackedImage)
  {
    // Debug.Log("stopped tracking");
    var videoPlaneParentGO = trackedImage.transform.GetChild(0).gameObject;
    var videoPlaneGO = videoPlaneParentGO.transform.GetChild(0).gameObject;
    var videoPlayer = videoPlaneGO.GetComponent<VideoPlayer>();
    if (videoPlayer != null)
    {
      videoPlayer.Stop();
      Debug.Log("stopped video");
    }
    videoPlaneGO.SetActive(false);
  }

  void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
  {
    foreach (var trackedImage in eventArgs.added)
    {
      // Give the initial image a reasonable default scale
      trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

      UpdateVideo(trackedImage);
    }

    foreach (var trackedImage in eventArgs.updated)
    {
      UpdateVideo(trackedImage);
    }

    foreach (var trackedImage in eventArgs.removed)
    {
      StopVideo(trackedImage);
    }
  }
}
