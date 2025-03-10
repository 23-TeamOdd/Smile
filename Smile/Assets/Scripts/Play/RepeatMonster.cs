using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MonsterSpawner에 들어있음
public class RepeatMonster : MonoBehaviour
{
    public GameObject[] monsters;
    static private GameObject monster;

    public Sprite[] monster_images;
    static private SpriteRenderer monster_image;

    private string[] monster_name = { "Rose", "Cosmos", "MorningGlory", "Poppy", "Pasque" };

    static public int monsterCount;
    static private int monsterOriginCount; // 몬스터 등장 횟수 (난이도에 따라 설정)

    public GameClear s_gameclear;

    public NoteController[] noteControllers;
    private NoteController noteController;

    // 몬스터 랜덤 배치
    //private static int ran_mon = 0;//HACK
    // 어떤 몬스터가 나올지 csv를 통해 읽어옴
    private static int choice_mon = 0;

    // Start is called before the first frame update
    void Start()
    {
        switch (UniteData.Difficulty)
        {
            case 1:
                monster = monsters[0];
                noteController = noteControllers[0];
                UniteData.data = CSVReader.Read("World1_easy");
                choice_mon = 0; // 장미
                break;

            case 2:
                monster = monsters[1];
                noteController = noteControllers[1];
                UniteData.data = CSVReader.Read("World1_normal");
                choice_mon = 1; // 코스모스
                break;

            case 3:
                monster = monsters[2];
                noteController = noteControllers[2];
                UniteData.data = CSVReader.Read("World1_hard");
                choice_mon = 1; // 코스모스
                break;

            case 4:
                monster = monsters[3];
                noteController = noteControllers[3];
                UniteData.data = CSVReader.Read("World2_easy");
                choice_mon = 3; // 양귀비
                break;

            case 5:
                monster = monsters[4];
                noteController = noteControllers[4];
                UniteData.data = CSVReader.Read("World2_normal");
                choice_mon = 3; // 양귀비
                break;

            case 6:
                monster = monsters[5];
                noteController = noteControllers[5];
                UniteData.data = CSVReader.Read("World2_hard");
                choice_mon = 2; // 나팔꽃
                break;

            case 7:
                monster = monsters[6];
                noteController = noteControllers[6];
                UniteData.data = CSVReader.Read("World_end");
                choice_mon = 0; // 장미
                break;

            default:
                monster = monsters[0];
                noteController = noteControllers[0];
                UniteData.data = CSVReader.Read("World1_easy");
                break;
        }

        monster_image = monster.GetComponent<SpriteRenderer>();

        if (UniteData.ReStart)
            Initialized();

        monster_image.sprite = monster_images[choice_mon];
    }

    // Update is called once per frame
    void Update()
    {
        //HACK
        //monster_image.sprite = monster_images[ran_mon]; 
        //Debug.Log("monster_image" + monster_image.sprite);
    }

    public void Initialized()
    {
        /*
        Debug.Log("몬스터 마리수 초기화");
        switch (UniteData.Difficulty)
        {
            case 1 :
                monsterCount = 6;
                break;

            case 2 :
                monsterCount = 7;
                break;

            case 3 :
                monsterCount = 10;
                break;

            default : 
                monsterCount = 6;
                break;
        }
        */
        // CSV를 읽어서 나오는 마리수 설정
        monsterOriginCount = int.Parse(UniteData.data[0]["num"].ToString());
        monsterCount = monsterOriginCount;
        UniteData.mon_num = 1;
        Debug.Log("monsterCountNew : "+  monsterCount);
        MonsterColorOrigin();
        UniteData.ReStart = false;
    }

    public void MonsterColorOrigin()
    {
        Debug.Log("monsterCount" + monsterCount);
        Debug.Log("MonsterColorOrigin");
        monster.SetActive(true);

        // 노트 초기화
        noteController.returnNote();

        /*// 몬스터 랜덤 배치
        int ran_mon = 0;//HACK*/

        /*if(UniteData.Difficulty == 2)
        {
            // 노말 모드에서만 코스모스 나오게
            ran_mon = Random.Range(0, 2);
        }

        if(monsterCount == 2 && UniteData.Difficulty == 2)
        {
            ran_mon = 1;
        }
        */
#if true
        Debug.Log("mon_num : " + UniteData.mon_num);
        if (UniteData.data[UniteData.mon_num]["Monster"].ToString().Equals("Rose"))
            choice_mon = 0;
        else if (UniteData.data[UniteData.mon_num]["Monster"].ToString().Equals("Cosmos")) 
            choice_mon = 1;
        else if (UniteData.data[UniteData.mon_num]["Monster"].ToString().Equals("MorningGlory"))
            choice_mon = 2;
        else if (UniteData.data[UniteData.mon_num]["Monster"].ToString().Equals("Poppy"))
            choice_mon = 3;
        else if (UniteData.data[UniteData.mon_num]["Monster"].ToString().Equals("Pasque"))
            choice_mon = 4;

        UniteData.Closed_Monster = monster_name[choice_mon];
        monster_image.sprite = monster_images[choice_mon];
        Debug.Log("monster_image" + monster_image.sprite);

        // 줄여놨던 a값 다시 원상 복귀
        Color c = monster.GetComponent<SpriteRenderer>().color;
        c.a = 225f;
        monster.GetComponent<SpriteRenderer>().color = c;
#endif
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MonsterSpwan"))
        {
            if (monsterCount > 0)
            {
                if (monsterCount != monsterOriginCount) // 맨처음에는 안나오게
                {
                    UniteData.mon_num++;
                    MonsterColorOrigin();   
                }              
                monsterCount--;
            }
            else if (monsterCount == 0)
            {
                Debug.Log("Game Clear");
                s_gameclear.ClearGame();
            }
        }
    }

}
