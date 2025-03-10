using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //[SerializeField] private int moveSpeed;

    public IScenePass scenePass;
    public GameObject Cut_Scene_prefab;

    private Color del_color = new Color(0, 0, 0);
    private Color show_color = new Color(1, 1, 1);

    // 기회 포인트 관련
    public GameObject[] go_notePoints; // 기회 포인트 오브젝트

    // 목숨 포인트 관련
    public GameObject[] go_lifePoints;

    // 플레이어 애니메이션
    Animator playerAinm;

    // 상단 노트 UI
    [Header("등장할 노트 배경")] public GameObject Note_Bg;

    // Start is called before the first frame update
    void Start()
    {
        scenePass = GetComponent<IScenePass>();
        scenePass.LoadSceneAsync("InGame-RN");
        playerAinm = GetComponent<Animator>();

        if (UniteData.lifePoint == 0)
        {
            Make_Invisible_UI();

            Animator fadeAnimator = GameObject.Find("FadeOut").GetComponent<Animator>();
            // 페이드 아웃 애니메이션 이후 게임 오버 씬을 전환합니다.
            fadeAnimator.SetBool("IsStartFade", true);
        }

        Initialized();
    }

    public void Initialized()
    {
        UniteData.Move_Progress = true;

        playerAinm.SetBool("IsMoving", true);

        for (int i = 0; i < go_notePoints.Length; i++)
        {
            go_notePoints[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < go_lifePoints.Length; i++)
        {
            go_lifePoints[i].gameObject.SetActive(true);
        }

        //목숨 개수 갱신하여 디스플레이
        foreach (GameObject go in go_notePoints) 
        { 
            go.GetComponent<Image>().color = del_color;
        }
        for(int x=0; x<UniteData.notePoint; x++)
        {
            go_notePoints[x].GetComponent<Image>().color = show_color;
        }

        foreach (GameObject go in go_lifePoints) 
        {
            go.GetComponent<Image>().color = del_color;
        }
        for(int x=0; x<UniteData.lifePoint; x++)
        {
            go_lifePoints[x].GetComponent<Image>().color = show_color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = transform.position + (transform.right * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
#if true //몬스터에 접근해도 멈추지 않도록 디버깅용 매크로
        if (!UniteData.NoteSuccess && collision.CompareTag("Monster"))
        {
            //moveSpeed = 0;
            Debug.Log("UniteData.notePoint" + UniteData.notePoint);

            // 상단 노트 UI 가리기
            Note_Bg.SetActive(false);

            // 기회가 남아있다면 씬 이동
            if (UniteData.notePoint > 0)
            {
                UniteData.notePoint--;

                // 몬스터에 닿으면 움직임을 멈춤
                UniteData.Move_Progress = false;

                // 플레이어 애니메이션 멈춤
                playerAinm.SetBool("IsMoving", false);

                //씬 애니메이션을 본 뒤 이동
                StartCoroutine(LoadCutScene());
            }

            // 기회가 0이라면 목숨 포인트 감소
            else if (UniteData.notePoint == 0)
            {
                Debug.Log("UniteData.lifePoint " + UniteData.lifePoint);
                // 목숨 포인트가 남아있다면 감소
                //if(UniteData.lifePoint >= 0)
                MeetMonsterFail();
            }

            
        }
#endif
    }

    public void MeetMonsterFail()
    {
        UniteData.lifePoint--;
        go_lifePoints[UniteData.lifePoint].GetComponent<Image>().color = del_color;

        // 0이 되면 게임 오버    
        if (UniteData.lifePoint == 0)
        {
            Make_Invisible_UI();

            Animator fadeAnimator = GameObject.Find("FadeOut").GetComponent<Animator>();
            // 페이드 아웃 애니메이션 이후 게임 오버 씬을 전환합니다.
            fadeAnimator.SetBool("IsStartFade", true);
        }
    }


    public static Vector3 CamabsolutePosition = new Vector3(0, 0, 0);
    private IEnumerator LoadCutScene()
    {
        Make_Invisible_UI();

        GameObject Cam = GameObject.Find("Main Camera");

        //카메라의 절대좌표를 가져온다
        CamabsolutePosition = Cam.transform.localPosition + new Vector3(0, 0, 10);

        //애니메이션 주기

        //컷씬 판 만들기
        //Instantiate(Cut_Scene_prefab, CamabsolutePosition, Quaternion.identity);
        Cut_Scene_prefab.transform.position = CamabsolutePosition;

        //애니메이션 시작
        Animator anim = Cut_Scene_prefab.GetComponent<Animator>();
        anim.SetBool("IsStart", true);

        //컷씬 애니메이션이 끝나면 씬 바로 이동
        yield return new WaitForSeconds(1.17f);
        scenePass.SceneLoadStart("InGame-RN");
    }

    private void Make_Invisible_UI()
    {
        //게임 오브젝트 중 UI_Touch Tag를 SetActive(false)로 설정한다
        GameObject[] UI_Touch = GameObject.FindGameObjectsWithTag("PlayScene_UI");
        foreach (GameObject UI in UI_Touch)
        {
            UI.SetActive(false);
        }

        GameObject Cam = GameObject.Find("Main Camera");
    }

}
