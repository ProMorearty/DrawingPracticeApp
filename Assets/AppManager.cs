using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.EventSystems;

public class AppManager : MonoBehaviour
{
    public InputField SecondsBeforeStart;
    public InputField SecondsPerImage;
    public InputField SecondsBetweenImages;
    public InputField SecondsPerAudioNotification;
    public InputField ImageDirectory;
    public GameObject ControlsPanel;
    public GameObject SettingsPanel;
    public Button Settings;
    private bool settingsEnabled;
    public Button PauseButton;
    private bool paused;
    public Button Start;
    public Button PreviousImage; // Implement
    public Button NextImage;// Implement
    public RawImage GUIImage;
    public Image Progress;
    private bool practicing;
    public Text Countdown;
    private SaveData saveData;
    public AudioClip valid;  
    public AudioClip invalid;
    private AudioSource audioSource;
    private bool canPing;
    private float pingTimer;
    public Text SessionTime;
    public Text ImageTime;
    private float sessionTimeAccum;
    private float imageTimeAccum;
    public Text ImagesShownText;
    private int ImagesShownAccum;
    public Toggle ManualImageSwitching;
    private bool changingToNextImage;
    public Toggle FlipXRandomly;
    public Toggle FlipYRandomly;
    private bool nextImage;
    private bool prevImage;
    public Toggle RandomizeImageOrder;
    public Sprite StartSprite;
    public Sprite StopSprite;

    // External Scripts
    private ImageLoader imageLoader;
    private ImageResizer imageResizer;

    private GameObject mainCanvas;

    private void Awake()
    {
        Application.runInBackground = true; // So it keeps playing when not selected, E.G. you are painting in Photoshop

        // Set Defaults
        SecondsBeforeStart.text = "5";
        SecondsPerImage.text = "30";
        SecondsBetweenImages.text = "3";
        SecondsPerAudioNotification.text = "0";

        SettingsPanel.SetActive(false);

        // External Scripts
        imageLoader = new ImageLoader();
        imageResizer = new ImageResizer();

        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");

        Start.onClick.AddListener(() => StartPractice());
        PauseButton.onClick.AddListener(() => TogglePause());
        PreviousImage.onClick.AddListener(() => UserPrevImage()); // Implement
        NextImage.onClick.AddListener(() => UserNextImage());
        Settings.onClick.AddListener(() => ToggleSettings());
        LoadSaveData();
        audioSource = Camera.main.GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        SaveSaveData();
    }

    public void MainPanelClicked()
    {
        //Dismiss settings panel if user clicks into empty space
        if (settingsEnabled)
        {
            ToggleSettings();
        }
    }

    private void StartPractice()
    {
        if (practicing)
        {
            StopPractice();
            return;
        }

        if ((ManualImageSwitching.isOn && string.IsNullOrEmpty(ImageDirectory.text)) || 
            (!ManualImageSwitching.isOn && (string.IsNullOrEmpty(SecondsPerImage.text) || string.IsNullOrEmpty(ImageDirectory.text) || Convert.ToInt32(SecondsPerImage.text) < 1)))
        {
            audioSource.PlayOneShot(invalid, 0.5f);
            return;
        }

        if (ManualImageSwitching.isOn)
        {
            SecondsBeforeStart.text = "0";
            SecondsPerImage.text = "0";
            SecondsBetweenImages.text = "0";
            SecondsPerAudioNotification.text = "0";
        }

        audioSource.PlayOneShot(valid, 0.5f);
        imageLoader.InitWithFolder(ImageDirectory.text);
        StartCoroutine("Practice");
    }

    private void TogglePause()
    {
        paused = !paused;

        if (!paused)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    private void ToggleSettings()
    {
        settingsEnabled = !settingsEnabled;

        // Think about handling pausing... and ignoring input from controls
        if (settingsEnabled)
        {
            if (paused)
            {
                TogglePause(); // unpause
            }

            if (practicing)
            {
                StopPractice(); // stop practice
            }

            SettingsPanel.SetActive(true); // enable settings panel

            ControlsPanel.SetActive(false); // disable other controls
        }
        else
        {
            SaveSaveData();

            SettingsPanel.SetActive(false); // disable settings panel

            ControlsPanel.SetActive(true); // enable other controls
        }
    }

    private void StopPractice()
    {
        if (!practicing)
        {
            return;
        }

        StopCoroutine("Practice");
        practicing = false;
        Countdown.gameObject.SetActive(false);
        GUIImage.gameObject.SetActive(false);
        SetCanPing(false);
        imageTimeAccum = 0;
        ImagesShownAccum = 0;
        Progress.fillAmount = 0f;
        Start.transform.Find("Image").GetComponent<Image>().sprite = StartSprite;
    }

    private IEnumerator Practice()
    {
        practicing = true;
        Start.transform.Find("Image").GetComponent<Image>().sprite = StopSprite;

        yield return WaitToStart();
        float timer;

        while (practicing)
        {
            if (!ManualImageSwitching.isOn)
            {
                timer = Convert.ToInt32(SecondsPerImage.text);
                yield return ChangeToNextImage();

                // Interruptable wait for SecondsPerImage
                while (timer > 0)
                {
                    timer -= Time.deltaTime;

                    if (nextImage)
                    {
                        nextImage = false;
                        Progress.fillAmount = 0;
                        break;
                    }
                    else if (prevImage)
                    {
                        //Implement
                    }

                    yield return null;
                }
            }
            else
            {
                yield return ChangeToNextImage(); // auto load first image

                while (ManualImageSwitching.isOn)
                {
                    if (nextImage)
                    {
                        nextImage = false;
                        Progress.fillAmount = 0;
                        if (!changingToNextImage)
                        {
                            StartCoroutine(ChangeToNextImage());
                        }
                    }
                    else if (prevImage)
                    {
                        //Implement
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

        }
    }

    private IEnumerator WaitToStart()
    {
        Countdown.gameObject.SetActive(true);
        GUIImage.gameObject.SetActive(false);

        float timer = Convert.ToInt32(SecondsBeforeStart.text);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Countdown.text = string.Format("Starting in {0}", Math.Ceiling(timer).ToString());
            yield return new WaitForEndOfFrame();
        }
    }

    private void UserNextImage()
    {
        if (practicing && !nextImage && !prevImage)
        {
            nextImage = true;
        }
    }

    private void UserPrevImage()
    {
        if (practicing && !prevImage && !nextImage)
        {
            prevImage = true;
        }
    }

    private IEnumerator ChangeToNextImage()
    {
        changingToNextImage = true;

        SetCanPing(false);
        Countdown.text = "Loading Image";
        Countdown.gameObject.SetActive(true);
        GUIImage.gameObject.SetActive(false);
        Progress.fillAmount = 0;
        yield return new WaitForEndOfFrame(); // So the changes above are shown.

        var tempImage = imageLoader.LoadNextImage(RandomizeImageOrder.isOn);
        imageResizer.Resize(mainCanvas, tempImage, GUIImage, FlipXRandomly);

        float timer = Convert.ToInt32(SecondsBetweenImages.text);
        while (timer > 0.1f)
        {
            timer -= Time.deltaTime;
            Countdown.text = string.Format("{0}", Math.Ceiling(timer).ToString());
            yield return new WaitForEndOfFrame();
        }

        GUIImage.texture = tempImage;
        Progress.fillAmount = 1f;
        SetCanPing(true);

        Countdown.gameObject.SetActive(false);
        GUIImage.gameObject.SetActive(true);
        IncrementImagesShown();
        imageTimeAccum = 0;

        changingToNextImage = false;
    }

    private void IncrementImagesShown()
    {
        ImagesShownAccum++;
        ImagesShownText.text = string.Format("Images Shown: {0}", ImagesShownAccum);
    }

    private void SetCanPing(bool b)
    {
        canPing = b;
        pingTimer = 0;
    }

    private void PlayPingSound()
    {
        pingTimer = 0;
        audioSource.PlayOneShot(invalid, 0.5f);
    }

    private void Update()
    {
        if (practicing)
        {
            Progress.fillAmount -= 1f / Convert.ToInt32(SecondsPerImage.text) * Time.deltaTime;
            sessionTimeAccum += Time.deltaTime;
            imageTimeAccum += Time.deltaTime;
            SessionTime.text = string.Format("Session Time: {0}s ", (int)sessionTimeAccum);
            ImageTime.text = string.Format("Image Time: {0}s ", (int)imageTimeAccum);
        }

        if (canPing && !string.IsNullOrEmpty(SecondsPerAudioNotification.text) && Convert.ToInt32(SecondsPerAudioNotification.text) > 0)
        {
            pingTimer += Time.deltaTime;
            if (pingTimer >= Convert.ToInt32(SecondsPerAudioNotification.text))
            {
                PlayPingSound();
            }
        }
    }

    public void LoadSaveData()
    {
        if (File.Exists(Application.dataPath + "/DrawingPracticeSaveData.json"))
        {
            string dataAsJson = File.ReadAllText(Application.dataPath + "/DrawingPracticeSaveData.json");
            saveData = JsonUtility.FromJson<SaveData>(dataAsJson);

            ImageDirectory.text = saveData.ImageDirectory;
            SecondsBeforeStart.text = saveData.SecondsBeforeStart;
            SecondsPerImage.text = saveData.SecondsPerImage;
            SecondsBetweenImages.text = saveData.SecondsBetweenImages;
            SecondsPerAudioNotification.text = saveData.SecondsPerAudioNotification;

            ManualImageSwitching.isOn = saveData.ManualImageSwitching;
            RandomizeImageOrder.isOn = saveData.RandomizeImageOrder;
            FlipXRandomly.isOn = saveData.FlipXRandomly;
            FlipYRandomly.isOn = saveData.FlipYRandomly;
            return;
        }
        saveData = new SaveData();
    }

    public void SaveSaveData()
    {
        saveData.ImageDirectory = ImageDirectory.text;
        saveData.SecondsBeforeStart = SecondsBeforeStart.text;
        saveData.SecondsPerImage = SecondsPerImage.text;
        saveData.SecondsBetweenImages = SecondsBetweenImages.text;
        saveData.SecondsPerAudioNotification = SecondsPerAudioNotification.text;

        saveData.ManualImageSwitching = ManualImageSwitching.isOn;
        saveData.RandomizeImageOrder = RandomizeImageOrder.isOn;
        saveData.FlipXRandomly = FlipXRandomly.isOn;
        saveData.FlipYRandomly = FlipYRandomly.isOn;

        string dataAsJson = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.dataPath + "/DrawingPracticeSaveData.json", dataAsJson);
    }
}

[Serializable]
public class SaveData
{
    public SaveData()
    {
        ImageDirectory = "";
        SecondsBeforeStart = "";
        SecondsPerImage = "";
        SecondsBetweenImages = "";
        SecondsPerAudioNotification = "";
        ManualImageSwitching = false;
        RandomizeImageOrder = false;
        FlipXRandomly = false;
        FlipYRandomly = false;
    }

    public string ImageDirectory;
    public string SecondsBeforeStart;
    public string SecondsPerImage;
    public string SecondsBetweenImages;
    public string SecondsPerAudioNotification;
    public bool ManualImageSwitching;
    public bool RandomizeImageOrder;
    public bool FlipXRandomly;
    public bool FlipYRandomly;
}
