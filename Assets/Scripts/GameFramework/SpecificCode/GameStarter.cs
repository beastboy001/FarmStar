using System.Collections;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    private StartArchitecture startArchitecture;

    void Awake()
    {
        Debug.Log("========== GameStarter.Awake() ==========");

        startArchitecture = StartArchitecture.Instance;
        PaintArchitercture architecture = new PaintArchitercture();
        startArchitecture.SetGameArchitecture(architecture);

        IArchitecture checkArch = startArchitecture.GetArchitecture();
        if (checkArch != null)
        {
            startArchitecture.InitAllModulesInArchitecture();
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("========== GameStarter.Start() ==========");

        IArchitecture architecture = StartArchitecture.Instance.GetArchitecture();
        if (architecture != null)
        {
            IUISystem uiSystem = architecture.GetSystem<IUISystem>();
            if (uiSystem != null)
            {
                uiSystem.Init();
            }
        }

        StartCoroutine(DelayAutoAcceptQuest());
    }

    IEnumerator DelayAutoAcceptQuest()
    {
        yield return new WaitForSeconds(0.5f);

        while (QuestManager.Instance == null)
            yield return null;

        Debug.Log("QuestManager 已就绪，开始检查任务");

        if (QuestManager.Instance != null)
        {
            // 修复：使用 questId 字符串，而不是 QuestData 对象
            string questId = "visit_village_head";
            QuestStatus status = QuestManager.Instance.GetQuestStatus(questId);

            if (status == QuestStatus.CanAccept)
            {
                QuestManager.Instance.AcceptQuest(questId);
                Debug.Log("✅ 自动接取任务：拜访村长爷爷");
            }
            else
            {
                Debug.Log($"任务 visit_village_head 当前状态为: {status}，无法自动接取");
            }
        }
    }

    public void Reinitialize()
    {
        startArchitecture = StartArchitecture.Instance;
        PaintArchitercture architecture = new PaintArchitercture();
        startArchitecture.SetGameArchitecture(architecture);
        startArchitecture.InitAllModulesInArchitecture();
    }
}