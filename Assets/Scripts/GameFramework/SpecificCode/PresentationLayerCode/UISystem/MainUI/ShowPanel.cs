using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class ShowPanel : BasePanel
{
    private Button DetailButton;
    private Button ExitButton;
    private Button LeftArrow;
    private Button RightArrow;

    private GameObject First;
    private GameObject Twist;
    private Text Name;
    private Text Post;
    private Text Project;
    private int index;

    private List<Information > people=new List<Information>();

    
    public override void OnInit()
    {
        base.OnInit();
        index = 0;
        First = transform.Find("First").gameObject;
        Twist = transform.Find("Twist").gameObject;
        First.SetActive(true);
        Twist.SetActive(false);
        people.Add(new Information("姓名：钟健翔（1/5）", "职务：项目负责人，主程序", "项目分工：搭建技术架构，实现核心功能与玩法逻辑"));
        people.Add(new Information("姓名：周俊杰（2/5）", "职务：技术美术，地编", "项目分工：负责场景地编，以及场景优化"));
        people.Add(new Information("姓名：王子文（3/5）", "职务：建模，策划", "项目分工：负责游戏策划，音效和建模"));
        people.Add(new Information("姓名：庞星熠（4/5）", "职务：原画", "项目分工：UI设计，美术资产绘制"));
        people.Add(new Information("姓名：陈思语（5/5）", "职务：文案，宣传", "项目分工：负责游戏文案以及宣传"));



        //寻找组件
        DetailButton = transform.Find("First/DetailButton").GetComponent<Button>();
        ExitButton = transform.Find("ExitButton").GetComponent<Button>();
        LeftArrow = transform.Find("Twist/LeftArrow").GetComponent<Button>();
        RightArrow = transform.Find("Twist/RightArrow").GetComponent<Button>();

        Name = transform.Find("Twist/Information/Name").GetComponent<Text>();
        Post = transform.Find("Twist/Information/Post").GetComponent<Text>();
        Project = transform.Find("Twist/Information/Project").GetComponent<Text>();
        
        Name.text  = people[index].Name;
        Post.text = people[index].Post;
        Project.text = people[index].Project;


        //添加监听
        DetailButton.onClick.AddListener(OnDetailButton);
        ExitButton.onClick.AddListener(OnExitButton);
        LeftArrow.onClick.AddListener(OnLeftArrow);
        RightArrow.onClick.AddListener(OnRightArrow);

    }
    private void OnDetailButton()
    {
        First.SetActive(false);
        Twist.SetActive(true);
    }
    private void OnExitButton()
    {
        First.SetActive(true);
        Twist.SetActive(false);
        index = 0;
        // 更新显示
        UpdateDisplay();

        OnClose();

    }
    private void OnLeftArrow()
    {
        // 向左：索引减1（显示上一条）
        index--;

        // 如果小于0，跳到最后一个
        if (index < 0)
        {
            index = people.Count - 1;
        }

        // 更新显示
        UpdateDisplay();
    }

    private void OnRightArrow()
    {
        // 向右：索引加1（显示下一条）
        index++;

        // 如果超出最后一个，回到第一个
        if (index >= people.Count)
        {
            index = 0;
        }

        // 更新显示
        UpdateDisplay();
    }

    // 添加一个更新显示的方法，避免重复代码
    private void UpdateDisplay()
    {
        Name.text = people[index].Name;
        Post.text = people[index].Post;
        Project.text = people[index].Project;
    }
    private class Information
    {
        public string Name { get; set; }
        public string Post { get; set; }
        public string Project { get; set; }


        // 构造函数（方便一行代码创建对象）
        public Information(string name, string post, string project)
        {
            Name = name;
            Post = post;
            Project = project;

        }
    }
}
