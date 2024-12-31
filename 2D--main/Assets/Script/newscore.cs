using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ScoreManager : MonoBehaviour
{
    public Text remainingTimeText;
    public Text moneyPointText;
    public Text secretCountText;
    public Text recordText;
    public Text pressButtonText;
    public InputField nameInputField; // �̸� �Է� �ʵ�
    public Button submitButton; // ���� ��ư
    public AudioClip transitionClip; // �� ��ȯ ���� ����� ����� Ŭ��
    public Image fadeImage;

    private float remainingTime;
    private int moneyPoint;
    private int secretCount;
    private AudioSource audioSource;
    private bool canPressButton = false;

    private List<Record> records = new List<Record>();
    private const int maxRecords = 12;

    private string[] pressButtonMessages = {
        "������Ϸ��� �ƹ� Ű�� �����ּ���",
        "�ƹ� Ű�� ������ �����",
        "�ƹ� Ű�� ������ ������Ͻʽÿ�",
        "����=�ƹ� Ű",
        "EŰ�� ������ ������ �����ϴ�."
    };

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // ������ �ҷ�����
        remainingTime = PlayerPrefs.GetFloat("RemainingTime", 0);
        moneyPoint = PlayerPrefs.GetInt("MoneyPoint", 0);
        secretCount = PlayerPrefs.GetInt("SecretCount", 0);

        // ��� �ҷ�����
        LoadRecords();

        // UI ������Ʈ
        remainingTimeText.text ="�ð�" +FormatTime(remainingTime);
        moneyPointText.text ="��" +moneyPoint.ToString();
        secretCountText.text = "��ũ��"+secretCount.ToString();
        recordText.text = GetRecordsText();

        // ������ �� ȭ���� �����ϰ� ��ȯ
        StartCoroutine(FadeIn());



        // ���� ��ư Ŭ�� �̺�Ʈ �߰�
        submitButton.onClick.AddListener(OnSubmitName);
    }

    void Update()
    {
        if (canPressButton && Input.anyKeyDown)
        {
            // ��� ���� ������� ����
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // �� ��ȯ ���� ����� Ŭ�� ���
            StartCoroutine(PlayTransitionClipAndLoadScene("Title"));
        }
    }

    void OnSubmitName()
    {
        string playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            // �ű�� ������Ʈ
            UpdateRecords(playerName, moneyPoint, remainingTime, secretCount);

            // �̸� �Է� UI �����
            nameInputField.gameObject.SetActive(false);
            submitButton.gameObject.SetActive(false);
        
            // ��� UI ������Ʈ
            recordText.text = GetRecordsText();
            // 5�� �Ŀ� "��ư�� ������ �˴ϴ�" �ؽ�Ʈ ǥ��
            canPressButton = true;
            StartCoroutine(ShowPressButtonText());
           
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60000);
        int seconds = Mathf.FloorToInt((time % 60000) / 1000);
        int milliseconds = Mathf.FloorToInt(time % 1000);
        return string.Format("{0:0}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    void LoadRecords()
    {
        records.Clear();
        for (int i = 0; i < maxRecords; i++)
        {
            string name = PlayerPrefs.GetString("RecordName" + i, "");
            int money = PlayerPrefs.GetInt("RecordMoney" + i, 0);
            float time = PlayerPrefs.GetFloat("RecordTime" + i, 0);
            int secret = PlayerPrefs.GetInt("RecordSecret" + i, 0);
            if (!string.IsNullOrEmpty(name))
            {
                records.Add(new Record(name, money, time, secret));
            }
        }
    }

    void SaveRecords()
    {
        for (int i = 0; i < records.Count; i++)
        {
            PlayerPrefs.SetString("RecordName" + i, records[i].name);
            PlayerPrefs.SetInt("RecordMoney" + i, records[i].money);
            PlayerPrefs.SetFloat("RecordTime" + i, records[i].time);
            PlayerPrefs.SetInt("RecordSecret" + i, records[i].secret);
        }
    }

    void UpdateRecords(string name, int money, float time, int secret)
    {
        records.Add(new Record(name, money, time, secret));
        records.Sort((a, b) =>
        {
            int secretComparison = b.secret.CompareTo(a.secret);
            if (secretComparison != 0)
            {
                return secretComparison;
            }
            return a.time.CompareTo(b.time);
        });
        if (records.Count > maxRecords)
        {
            records.RemoveAt(records.Count - 1);
        }
        SaveRecords();
    }

    string GetRecordsText()
    {
        string text = "< �ű�� >\n\n";
        for (int i = 0; i < records.Count; i++)
        {
            text += $"{i + 1}�� {records[i].name} - ��: {records[i].money}, �ð�: {FormatTime(records[i].time)}, ��ũ��: {records[i].secret}\n";
        }
        return text;
    }

    IEnumerator FadeIn()
    {
        float duration = 1.0f;
        float currentTime = 0f;

        Color color = fadeImage.color;
        while (currentTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            currentTime += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, 0f);
    }

    IEnumerator ShowPressButtonText()
    {
        yield return new WaitForSeconds(5f);
        pressButtonText.text = pressButtonMessages[Random.Range(0, pressButtonMessages.Length)];
        canPressButton = true;
    }

    IEnumerator PlayTransitionClipAndLoadScene(string sceneName)
    {
        if (transitionClip != null)
        {
            audioSource.PlayOneShot(transitionClip);
            yield return new WaitForSeconds(transitionClip.length);
        }
        SceneManager.LoadScene(sceneName);
    }
}

[System.Serializable]
public class Record
{
    public string name;
    public int money;
    public float time;
    public int secret;

    public Record(string name, int money, float time, int secret)
    {
        this.name = name;
        this.money = money;
        this.time = time;
        this.secret = secret;
    }
}