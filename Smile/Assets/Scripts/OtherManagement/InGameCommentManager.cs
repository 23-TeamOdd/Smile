using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class InGameCommentManager : MonoBehaviour
{
    TSV tsv;
    DataTable dataTable;
    DataRowCollection dataRowCollection;
    UI_Comment uiComment;
    Animator animator;

    public GraphicRaycaster graphicRaycaster;
    public EventSystem eventSystem;

    public GameObject textGroup;
    public GameObject blind;
    public GameObject[] speakerPosition;
    public Text text;
    public Text speakerText;
    public GameObject stopPanel;

    public GameObject selectGroup;
    public GameObject[] buttonInSelectGroup;

    public Sprite[] speakerSprites;
    public Sprite[] branchSprites;

    public GameObject[] UI_system;
    public GameObject[] gameCharacters;

    private Dictionary<string, GameObject> inGame_characters = new Dictionary<string, GameObject>();
    private Dictionary<string, Sprite> characterSprites = new Dictionary<string, Sprite>();
    private void Awake()
    {
        characterSprites.Add("민들레", speakerSprites[0]);
        characterSprites.Add("튤립", speakerSprites[1]);
        characterSprites.Add("물망초", speakerSprites[2]);

        inGame_characters.Add("Dandelion", gameCharacters[0]);
        inGame_characters.Add("Tulip", gameCharacters[1]);
        inGame_characters.Add("ForgetMeNot", gameCharacters[2]);

        Debug.LogError("빌드 전에 !꼭! play/canvas/comment 오브젝트를 비활성화 해주세요!!!!!");

        animator = blind.GetComponent<Animator>();
    }


    private const int COMMAND   = 0;
    private const int NUMBERING = 1;
    private const int GOTO      = 2;
    private const int BRANCH    = 3;
    private const int IMAGEPOSITION =4;
    private const int IMAGETYPE=5;
    private const int CHARACTER = 6;
    private const int COMMENT   = 7;

    private const int LEFT = 0;
    private const int CENTER = 1;
    private const int RIGHT = 2;

    private Vector2[] characterImagePosition = { 
        new Vector2(-1200f, -90f), 
        new Vector2(0f, -90f), 
        new Vector2(1200f, -90f) 
    };

    private Color whoIs= new Color(0f, 0f, 0f, 1f);
    private Color ohItsYou = new Color(1f, 1f, 1f, 1f);
    private Color noOneIsHere = new Color(0f, 0f, 0f, 0f);
    private Color iWillListen = new Color(0.7f,0.7f, 0.7f, 1f);

    private int page = 0;
    private int pageEnd = 0; //1:End

    private int frameTime = 0;
    private const int FPP = 4;
    private bool clickTemp = false;
    private bool printAll = false;

    private void dataResetBeforeStartScripting()
    {
        page = 0;
        pageEnd = 0;
        frameTime = 0;
        clickTemp = false;
        printAll = false;
    }

    private string[] tsv_file = {
        "newType_W1E.tsv",
        "newType_W1E.tsv",
        "newType_W1E.tsv",
        "newType_W1E.tsv",
        "newType_W1E.tsv",
        "newType_W1E.tsv"
    };

    private void do_ThrowOutObject()
    {
        Time.timeScale = 0f; //이거 때문에 페이드 처리가 안됨
        Application.targetFrameRate = 60;
        foreach (GameObject obj in UI_system)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in gameCharacters)
        {
            obj.SetActive(false);
        }
    }
    private void do_BringInObject()
    {
        Time.timeScale = 1f;
        UniteData.GameMode = "Play";
        foreach (GameObject obj in UI_system)
        {
            obj.SetActive(true);
        }
        foreach (var entry in inGame_characters)
        {
            if (entry.Key == UniteData.Selected_Character)
            {
                entry.Value.SetActive(true);
                break;
            }
        }
        textGroup.SetActive(false);
    }

    private void OnEnable()
    {
        UniteData.GameMode = "Scripting";

        dataResetBeforeStartScripting();
        ////스토리 시작 전 오브젝트 비활성화
        do_ThrowOutObject();

        ////파일 지정
        uiComment = GetComponent<UI_Comment>();
        tsv = new TSV(tsv_file[UniteData.Difficulty - 1]);


        ////스토리 파일 내에 구간에 따라 텍스트를 미리 로드한다. [함수로 분할]
        if (UniteData.StoryClear[UniteData.Difficulty - 1] == 0 && !UniteData.finishGame)
        {
            dataRowCollection = startScripting("Prestart");
        }
        else if (UniteData.StoryClear[UniteData.Difficulty - 1] == 1 && UniteData.finishGame)
        {
            dataRowCollection = startScripting("Finish");
        }
        else //에러 처리
        {
            //스토리 끝 [함수로 분할]
            do_BringInObject();
            Debug.LogError("스토리 파일에 해당 Command가 없습니다.");
            return;
        }

        handleSelectGroup(3, false);
        clickTemp = true;
        outputScript(page, printAll);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 클릭 위치에서 레이캐스트 수행
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);

            // 레이캐스트가 특정 UI 오브젝트와 충돌하면
            if (results.Count > 0)
            {
                bool checkingBackBtn = false;
                foreach(var selectedResult in results)
                {
                    GameObject selectedObject= selectedResult.gameObject;
                    if (selectedObject.name == "BtnStop" || selectedObject.name == "No")
                    {
                        checkingBackBtn = true;
                        break;
                    }
                }


                if(!checkingBackBtn && !stopPanel.activeSelf) 
                {
                    if (pageEnd == 1)  //하나의 스크립트가 모두 출력이 완료했을 때.
                    {
                        clickTemp = false;
                    }

                    if (!clickTemp) //다음 스크립트로 넘어간다.
                    {
                        if (dataRowCollection[page+1][BRANCH].ToString() != "select")//만약 다음이 분기가 아니라면!
                        {
                            //다음 스크립트로 진행한다.
                            initAboutTextValues();
                            page = int.Parse(dataRowCollection[page][GOTO].ToString());
                            clickTemp = true;
                        }
                    }
                    else //printing immediately
                    {
                        printAll = true;
                        clickTemp = false;
                    }
                }
            }
        }

        outputScript(page, printAll);
        frameTime ++;
    }

    private void initAboutTextValues()
    {
        frameTime = 0;
        printAll = false;
    }


    public DataRowCollection startScripting(string command)
    {
        dataTable = tsv.limitTSV(command);
        return dataTable.Rows;
    }


    private void do_Branching(int rowX, DataRow nextRow)
    {
        handleSelectGroup(3, false); //초기 분기 선택 버튼 비활성화

        //분기 시작
        if (nextRow[BRANCH].ToString() == "select")
        {
            List<DataRow> selRow = new List<DataRow>();
            for (int x = 0; ; x++) //리스트에 분기 항목 넣기
            {
                if (dataRowCollection[rowX + x][BRANCH].ToString() == "select")
                {
                    selRow.Add(dataRowCollection[rowX + x]);
                }
                else
                {
                    break;
                }
            }

            for (int x = 0; x < selRow.Count; x++) //버튼에 분기 항목 넣고 활성화
            {
                handleSelectGroup(x + 1, true);
                buttonInSelectGroup[x].GetComponentInChildren<Text>().text = selRow[x][COMMENT].ToString();
            }

            //여기서 분기의 개수에 따라 분기 선택 버튼의 위치를 중앙으로 변경한다.
        }
    }


    private void do_ImageSetting(DataRow row, int loc)
    {
        if (row[IMAGETYPE].ToString() == row[CHARACTER].ToString())
        {
            speakerPosition[loc].GetComponent<Image>().color = ohItsYou;
            speakerPosition[loc].GetComponent<Image>().sprite = speakersBannerImage(row[IMAGETYPE].ToString());
            if (speakerPosition[loc].GetComponent<Image>().sprite == null)
            {
                speakerPosition[loc].GetComponent<Image>().color = noOneIsHere;
                return;
            }
        }
        else if (row[CHARACTER].ToString() == "???")
        {
            speakerPosition[loc].GetComponent<Image>().color = whoIs;
            speakerPosition[loc].GetComponent<Image>().sprite = speakersBannerImage(row[IMAGETYPE].ToString());
        }
    }

    private void attach_CharacterImage(DataRow row)
    {
        if (row[IMAGEPOSITION].ToString()=="CenterAlong")
        {
            speakerPosition[LEFT].GetComponent<Image>().color = noOneIsHere;
            speakerPosition[RIGHT].GetComponent<Image>().color = noOneIsHere;

            do_ImageSetting(row, CENTER);
        }
        else if(row[IMAGEPOSITION].ToString() == "RightTogether")
        {
            speakerPosition[CENTER].GetComponent<Image>().color = noOneIsHere;

            do_ImageSetting(row, RIGHT);

            //Right 하이라이트
        }
        else if (row[IMAGEPOSITION].ToString() == "LeftTogether")
        {
            speakerPosition[CENTER].GetComponent<Image>().color = noOneIsHere;

            do_ImageSetting(row, LEFT);

            //Left 하이라이트
        }
        else
        {
            return;
        }

        speakerText.text = row[CHARACTER].ToString();
    }
        
    private void outputScript(int rowX, bool isPrintingImmadiately)
    {
        if (rowX + 1 >= dataRowCollection.Count)//더 이상 출력할 것이 없다면
        {
            ////스토리 끝. 오브젝트 회기
            do_BringInObject();

            UniteData.StoryClear[UniteData.Difficulty - 1] += 1;
            UniteData.SaveUserData();
            return;
        }
        DataRow row = dataRowCollection[rowX];

        if (dataRowCollection[rowX][CHARACTER].ToString() == "System") 
        {
            if(dataRowCollection[rowX][COMMENT].ToString()=="Blind")
            {
                //블라인드 작업 지시
                Debug.Log("Blind");
            }
            return;
        }

        handleSelectGroup(3, false); //초기 분기 선택 버튼 비활성화

        do_Branching(rowX + 1, dataRowCollection[rowX + 1]);

        pageEnd = uiComment.printTextToUI(text, row[COMMENT].ToString(), frameTime, FPP, isPrintingImmadiately); //텍스트 출력

        attach_CharacterImage(row);
    }

    //분기 구현

    private Sprite speakersBannerImage(string character)
    {
        if (characterSprites.TryGetValue(character, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            return null;
        }
    }

    private void handleSelectGroup(int buttonAmount, bool isActive)
    {
        selectGroup.SetActive(isActive);
        for(int x=0; x<buttonAmount; x++)
        {
            buttonInSelectGroup[x].SetActive(isActive);
        }
    }

    public void branchInStoryByUsersSelecting(int buttonCode)
    {
        //코드에 맞는 Row를 가져와 
        //Goto를 추출해서 부여
        initAboutTextValues();
        clickTemp = true;
        handleSelectGroup(3, false);
        page = int.Parse(dataRowCollection[page+buttonCode][GOTO].ToString());
    }


    //
}
