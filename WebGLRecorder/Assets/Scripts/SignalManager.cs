using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SignalManager : MonoBehaviour
{
    public Button StartRecorder;
    public Button EndRecorder;
    AudioSource m_audioSource;
    void Start()
    {
        m_audioSource = gameObject.AddComponent<AudioSource>();
        StartRecorder.onClick.AddListener(StartRecorderFunc);
        EndRecorder.onClick.AddListener(EndRecorderFunc);
    }

    #region UnityToJs
    [DllImport("__Internal")]
    private static extern void StartRecord();
    [DllImport("__Internal")]
    private static extern void StopRecord();
    void StartRecorderFunc()
    {
        StartRecord();
    }
    void EndRecorderFunc()
    {
        StopRecord();
    }
    #endregion

    #region JsToUnity
    #region Data
    /// <summary>
    ///���ȡ���ݵ���Ŀ
    /// </summary>
    private int m_valuePartCount = 0;
    /// <summary>
    /// ��ȡ��������Ŀ
    /// </summary>
    private int m_getDataLength = 0;
    /// <summary>
    /// ��ȡ�����ݳ���
    /// </summary>
    private int m_audioLength = 0;
    /// <summary>
    /// ��ȡ������
    /// </summary>
    private string[] m_audioData = null;

    /// <summary>
    /// ��ǰ��Ƶ
    /// </summary>
    public static AudioClip m_audioClip = null;

    /// <summary>
    /// ��ƵƬ�δ���б�
    /// </summary>
    private List<byte[]> m_audioClipDataList;

    /// <summary>
    /// Ƭ�ν������
    /// </summary>
    private string m_currentRecorderSign;
    /// <summary>
    /// ��ƵƵ��
    /// </summary>
    private int m_audioFrequency;

    /// <summary>
    /// �������¼��ʱ��
    /// </summary>
    private const int maxRecordTime = 30;
    #endregion

    public void GetAudioData(string _audioDataString)
    {
        if (_audioDataString.Contains("Head"))
        {
            string[] _headValue = _audioDataString.Split('|');
            m_valuePartCount = int.Parse(_headValue[1]);
            m_audioLength = int.Parse(_headValue[2]);
            m_currentRecorderSign = _headValue[3];
            m_audioData = new string[m_valuePartCount];
            m_getDataLength = 0;
            Debug.Log("��������ͷ��" + m_valuePartCount + "   " + m_audioLength);
        }
        else if (_audioDataString.Contains("Part"))
        {
            string[] _headValue = _audioDataString.Split('|');
            int _dataIndex = int.Parse(_headValue[1]);
            m_audioData[_dataIndex] = _headValue[2];
            m_getDataLength++;
            if (m_getDataLength == m_valuePartCount)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < m_audioData.Length; i++)
                {
                    stringBuilder.Append(m_audioData[i]);
                }
                string _audioDataValue = stringBuilder.ToString();
                Debug.Log("���ճ���:" + _audioDataValue.Length + " ����ճ���:" + m_audioLength);
                int _index = _audioDataValue.LastIndexOf(',');
                string _value = _audioDataValue.Substring(_index + 1, _audioDataValue.Length - _index - 1);
                byte[] data = Convert.FromBase64String(_value);
                Debug.Log("�ѽ��ճ��� :" + data.Length);

                if (m_currentRecorderSign == "end")
                {
                    int _audioLength = data.Length;
                    for (int i = 0; i < m_audioClipDataList.Count; i++)
                    {
                        _audioLength += m_audioClipDataList[i].Length;
                    }
                    byte[] _audioData = new byte[_audioLength];
                    Debug.Log("�ܳ��� :" + _audioLength);
                    int _audioIndex = 0;
                    data.CopyTo(_audioData, _audioIndex);
                    _audioIndex += data.Length;
                    Debug.Log("�Ѹ�ֵ0:" + _audioIndex);
                    for (int i = 0; i < m_audioClipDataList.Count; i++)
                    {
                        m_audioClipDataList[i].CopyTo(_audioData, _audioIndex);
                        _audioIndex += m_audioClipDataList[i].Length;
                        Debug.Log("�Ѹ�ֵ :" + _audioIndex);
                    }

                    WAV wav = new WAV(_audioData);
                    AudioClip _audioClip = AudioClip.Create("TestWAV", wav.SampleCount, 1, wav.Frequency, false);
                    _audioClip.SetData(wav.LeftChannel, 0);

                    m_audioClip = _audioClip;
                    Debug.Log("��Ƶ���óɹ�,�����õ�unity��" + m_audioClip.length + "  " + m_audioClip.name);

                    m_audioSource.clip = m_audioClip;
                    m_audioSource.Play();

                    m_audioClipDataList.Clear();
                }
                else
                    m_audioClipDataList.Add(data);

                m_audioData = null;
            }
        }
    }
    #endregion
}