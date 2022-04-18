using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class QuestionBuilder : MonoBehaviour
{
    [Serializable]
    public class Record
    {
        public string question;
        public List<string> answers;
        public string explain;
        public string category;
        public int answerIndex;
    }

    [Serializable]
    public class Root
    {
        public List<Record> records;
    }

    private Root _questions;
    public TextAsset QJson;
    public UIDocument UiDoc;

    public BattleHud battleHud;
    private VisualElement _rootEle;

    VisualElement RootElement
    {
        get
        {
            if (_rootEle == null)
            {
                _rootEle = GetComponent<UIDocument>().rootVisualElement;
            }
            return _rootEle;
        }
        set
        {
            _rootEle = value;
        }
    }
    private VisualElement _controlsContainer;

    VisualElement ControlContainer
    {
        get
        {
            if (_controlsContainer == null)
            {
                _controlsContainer = RootElement.Q<VisualElement>("ControlsContainer");
            }
            return _controlsContainer;
        }
        set
        {
            _controlsContainer = value;
        }
    }

    private IEnumerator DownloadQuestions()
    {
        using (var webRequest = UnityWebRequest.Get("https://mfpd1xxqx7.execute-api.us-east-2.amazonaws.com//QA/QA/Search?count=9999&category=math"))
        {
            yield return webRequest.SendWebRequest();
            _questions = JsonUtility.FromJson<Root>(webRequest.downloadHandler.text);
            var rnd = new System.Random();
            _questions.records = _questions.records.OrderBy(question => rnd.Next()).ToList();
            _questions.records.ForEach(record =>
            {
                var answer = record.answers[0];
                record.answers = record.answers.OrderBy(ans => rnd.Next()).ToList();
                record.answerIndex = record.answers.IndexOf(answer);
            });
        }
    }

    private void RequestQuestions()
    {
        StartCoroutine(DownloadQuestions());
    }

    public Root GetQuestions()
    {
        if (_questions == null)
            RequestQuestions();
        return _questions;
    }

    // Start is called before the first frame update
    void Start()
    {
        _questions = GetQuestions();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
