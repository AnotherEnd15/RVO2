﻿using System;
using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Experimental.UIElements;
using Random = System.Random;


public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;

    [HideInInspector] public Vector3 mousePosition;

    private Plane mhPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, GameAgent> magentMap = new Dictionary<int, GameAgent>();

    // Use this for initialization
    void Start()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, Vector3.zero);
        // add in awake
        Simulator.Instance.processObstacles();
    }

    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (mhPlane.Raycast(mouseRay, out rayDistance))
            position = mouseRay.GetPoint(rayDistance);

        mousePosition.x = position.x;
        mousePosition.y = position.z;
    }

    void DeleteAgent()
    {

        int agentNo = Simulator.Instance.queryNearAgent(mousePosition, 1.5f);
        if (agentNo == -1 || !magentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        LeanPool.Despawn(magentMap[agentNo].gameObject);
        magentMap.Remove(agentNo);
    }
    void CreatAgent(Vector3 v3)
    {
        int sid = Simulator.Instance.addAgent(v3);
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, v3, Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            magentMap.Add(sid, ga);
        }
    }
    void CreatAgent(RVOLayer layer,RVOLayer collideWith,int priority = 1)
    {
        int sid = Simulator.Instance.addAgent(mousePosition, layer, collideWith, priority);
       // Simulator.Instance.setAgentMaxSpeed(sid, Simulator.Instance.getAgentMaxSpeed(sid) * priority);
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            go.GetComponentInChildren<MeshRenderer>().material.color = priority == 1 ? Color.white : Color.red; 
            Assert.IsNotNull(ga);
            ga.sid = sid;
            magentMap.Add(sid, ga);
        }
        
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateMousePosition();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("创建默认层");
            CreatAgent(RVOLayer.DefaultAgent, ~RVOLayer.Layer2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("创建第二层");
            CreatAgent(RVOLayer.Layer2, ~RVOLayer.DefaultAgent);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("创建默认层");
            CreatAgent(RVOLayer.DefaultAgent, ~RVOLayer.Layer2,2);
        }

        Simulator.Instance.doStep();
    }
}