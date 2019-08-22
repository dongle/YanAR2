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
[RequireComponent(typeof(VideoPlayer))]

public class TrackedImageVideoManager : MonoBehaviour
{
  ARTrackedImageManager m_TrackedImageManager;

  VideoPlayer m_videoPlayer;
  AudioSource m_audioSource;

  [SerializeField]
  RenderTexture m_videoTexture;
  public RenderTexture videoTexture
  {
    get { return m_videoTexture; }
    set { m_videoTexture = value; }
  }

  [SerializeField]
  Material m_videoMaterial;
  public Material videoMaterial
  {
    get { return m_videoMaterial; }
    set { m_videoMaterial = value; }
  }

  void Awake()
  {
    m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    m_audioSource = gameObject.AddComponent<AudioSource>();
    m_videoPlayer = GetComponent<VideoPlayer>();
    m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
  }

  void OnEnable()
  {
    m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    m_videoPlayer.prepareCompleted += ReadyVideo;
  }

  void OnDisable()
  {
    m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    m_videoPlayer.prepareCompleted -= ReadyVideo;
  }

  void UpdateVideo(ARTrackedImage trackedImage)
  {
    if (trackedImage.trackingState != TrackingState.None)
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

        // double the size of the video plane if it is a video that 'breaks' the frame
        if (trackedFrame == 2 || trackedFrame == 4 || trackedFrame == 5 || trackedFrame == 6 || trackedFrame == 9 || trackedFrame == 10 || trackedFrame == 12)
        {
          videoPlaneGO.transform.localScale = new Vector3(0.2f, 2f, 0.2f);
        }

        videoPlaneGO.SetActive(true);
        VideoClip clip = Resources.Load<VideoClip>(trackedFrameString) as VideoClip;
        m_videoPlayer.clip = clip;
        m_videoTexture.Release();
        m_videoPlayer.Prepare();
      }
    }
    else
    {
      StopVideo(trackedImage);
    }
  }

  void ReadyVideo(VideoPlayer source)
  {
    m_videoPlayer.Play();
  }

  void StopVideo(ARTrackedImage trackedImage)
  {
    Debug.Log("stopped tracking");
    var videoPlaneParentGO = trackedImage.transform.GetChild(0).gameObject;
    var videoPlaneGO = videoPlaneParentGO.transform.GetChild(0).gameObject;
    videoPlaneGO.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
    m_videoPlayer.Stop();
    m_videoTexture.Release();
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
