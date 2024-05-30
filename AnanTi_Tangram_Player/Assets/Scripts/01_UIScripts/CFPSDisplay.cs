using UnityEngine;
using System.Collections;

public class CFPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;

    private float fTimeElapsed = 0.0f;

    GUIStyle style;
    Rect rect;
    float msec;
    float fps;
    float worstFps = 100f;
    string text;

    void Awake()
    {

        int w = Screen.width, h = Screen.height;
        rect = new Rect(0, 0, w, h * 4 / 100);
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 4 / 100;
        style.normal.textColor = Color.red;

        StartCoroutine("worstReset");
    }


    IEnumerator worstReset() //�ڷ�ƾ���� 15�� �������� ���� ������ ��������.
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);
            worstFps = 100f;
        }
    }


    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        fTimeElapsed += Time.deltaTime;
    }

    void OnGUI()//�ҽ��� GUI ǥ��.
    {
    }

}


