using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    public Transform onObj;
    public Transform offObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offObj = transform.GetChild(0).GetComponent<Transform>();
        onObj = transform.GetChild(1).GetComponent<Transform>();

        onObj.GetComponent<Button>().onClick.AddListener(Pause);
        offObj.GetComponent<Button>().onClick.AddListener(Play);
    }

    public void Pause()
    {
        onObj.transform.gameObject.SetActive(false);
        offObj.transform.gameObject.SetActive(true);
        GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().Pause();
    }

    public void Play()
    {
        onObj.transform.gameObject.SetActive(true);
        offObj.transform.gameObject.SetActive(false);
        GameObject.Find("BackgroundMusic").GetComponent<AudioSource>().UnPause();
    }

    // Update is called once per frame
    void Update() { }
}
