﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NANDGate : MonoBehaviour, LogicInterface {
    private Dictionary<string, GameObject> logic_dictionary = new Dictionary<string, GameObject>(); //Contains all the gameobject nodes for the 74LS400 chip.+
    private GameObject DeviceGameObject;
    private GameObject snapIndicatorGameObj;
    private const string LOGIC_DEVICE_ID = "74LS00_NAND_NODE_";
    private Vector3 screenPoint;
    private Vector3 offset;
    private bool SNAPPED = false; //Set to true if all Logic Nodes of this device is in collision with an external node

    private void setNodeProperties(GameObject logicNode, string logicNodeID)
    {
        LogicNode logic_behavior = logicNode.AddComponent<LogicNode>() as LogicNode; //Adds the LogicNode.cs component to this gameobject to control logic behavior
        logic_behavior.SetLogicId(logicNodeID); //logic id that sets all the nodes on the left column of the LEFT section of the protoboard the same id
        logic_behavior.SetLogicNode(logicNode);
        logic_behavior.SetOwningDevice(this);
        SpriteRenderer sprite_renderer = logicNode.AddComponent<SpriteRenderer>(); //adds a test "circle" graphic
        sprite_renderer.sprite = Resources.Load<Sprite>("logicCircle");
        sprite_renderer.sortingLayerName = "Logic";
        BoxCollider2D box_collider = logicNode.AddComponent<BoxCollider2D>();
        box_collider.size = new Vector2(1f, 1f);
        box_collider.isTrigger = true;
        Rigidbody2D rigidbody = logicNode.AddComponent<Rigidbody2D>();
        rigidbody.isKinematic = true;

    }

    // Use this for initialization
    void Start()
    {
        DeviceGameObject = GameObject.Find("74LS00");
        //Loop that places Logic Nodes on the 74LS400 chip
        float horizontal_pos = -.205f; //set up for left side of the chip
        float vertical_pos = .58f; //top of the chip
        float vertical_direct = -.208f;
        for (int i = 0; i < 14; i++)
        {
            GameObject logicNode = new GameObject(LOGIC_DEVICE_ID + i); //logic node with the name leftlogicnode_{i}_0
            logicNode.transform.parent = DeviceGameObject.transform; //sets the Protoboard game object as logicNode_0's parent
            logicNode.transform.localPosition = new Vector3(horizontal_pos, vertical_pos + i * (vertical_direct), 0); //'localPosition' sets the position of this node RELATIVE to the protoboard
            logicNode.transform.localScale = new Vector3(.10F, .10F, 0);
            setNodeProperties(logicNode, LOGIC_DEVICE_ID + i);
            logic_dictionary.Add(LOGIC_DEVICE_ID + i, logicNode);
            if (i == 6) //when the left side is complete
            {
                vertical_pos = vertical_pos + (13 * vertical_direct);
                vertical_direct = .208f;
                horizontal_pos = horizontal_pos + .532f; //change the horizontal position to the right side

            }
        }

        //add SNAP indicator object to the chip
        snapIndicatorGameObj = new GameObject(LOGIC_DEVICE_ID + "_SNAP_INDICATOR_");
        snapIndicatorGameObj.transform.parent = DeviceGameObject.transform; //sets the Protoboard game object as logicNode_0's parent
        snapIndicatorGameObj.transform.localPosition = new Vector3(-.0775f, .575f, 0); //'localPosition' sets the position of this node RELATIVE to the protoboard
        snapIndicatorGameObj.transform.localScale = new Vector3(.10F, .10F, 0);
        SpriteRenderer sprite_renderer = snapIndicatorGameObj.AddComponent<SpriteRenderer>(); //adds a test "circle" graphic
        sprite_renderer.sprite = Resources.Load<Sprite>("logicCircle");
        sprite_renderer.sortingLayerName = "FrontLayer";
        sprite_renderer.material.color = new Color(1, 1, 1);
    }


    void OnMouseDown()
    {
        Debug.Log("74LS00 Mouse Down");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        transform.position = curPosition;

        //Check for snapping when chip is removed from set place.
        //Check if all nodes with the chip is colliding with another logic node;
        SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
        foreach (KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logic_node = entry.Value;
            LogicNode logic_behavior = logic_node.GetComponent<LogicNode>();
            if (logic_behavior.GetCollidingNode() == null)
            {
                //indicator
                spr_ren.material.color = new Color(1, 1, 1); //neutral
                SNAPPED = false;
                Debug.Log("Snap not set.");
                return;
            }
        }
        //if execution reached here, it means all colliding nodes are valid nodes
        //indicate device can be active
        spr_ren.material.color = new Color(0, 1, 0); //green

    }

    //The device is on if gnd and vcc have the correct logical inputs
    private bool IsDeviceOn()
    {
        GameObject logic_gnd;
        GameObject logic_vcc;
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 6, out logic_gnd) 
            && logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 13, out logic_vcc))
        {
            LogicNode logic_behavior_gnd = logic_gnd.GetComponent<LogicNode>();
            LogicNode logic_behavior_vcc = logic_vcc.GetComponent<LogicNode>();
            Debug.Log("GND Set to: " + logic_behavior_gnd.GetLogicState());
            Debug.Log("VCC Set to: " + logic_behavior_vcc.GetLogicState());
            if (logic_behavior_gnd.GetLogicState() == (int)LOGIC.LOW 
                && logic_behavior_vcc.GetLogicState() == (int)LOGIC.HIGH)
            {
                Debug.Log(this.DeviceGameObject.name + " is ON.");
                return true;
            }
        }
        Debug.Log(this.DeviceGameObject.name + " is OFF.");
        return false;
    }


    //Method that handles the input and output nodes based on the collisions
    private void ChipIO()
    {
        GameObject logic_0, logic_1, logic_2, logic_3, logic_4, logic_5, logic_6,
        logic_7,logic_8, logic_9, logic_10, logic_11, logic_12, logic_13;

        //GND
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 6, out logic_6))
        {
            LogicNode logic_behavior = logic_6.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            //GND pin collision node is not GND
            if(collided_state != (int)LOGIC.LOW)
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS00 Ground Input not set to LOW");
            }
            //GND pin collision node is also GND
            else 
            {
                logic_behavior.SetLogicState((int)LOGIC.LOW);

            }
        }
        //VCC
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 13, out logic_13))
        {
            LogicNode logic_behavior = logic_13.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if(collided_state != (int)LOGIC.HIGH)
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS00 VCC Input not set to HIGH");
            }
            else
            {
                logic_behavior.SetLogicState((int)LOGIC.HIGH);
            }
        }
        /**
         * INPUTs find the collided nodes of the input pins and sets the input's
         * pin state to the collided node's state.
         * 
         */
        //NAND INPUT 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 0, out logic_0))
        {
            LogicNode logic_behavior = logic_0.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if(collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 0 is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND INPUT 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 1, out logic_1))
        {
            LogicNode logic_behavior = logic_1.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 1 is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND INPUT 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 3, out logic_3))
        {
            LogicNode logic_behavior = logic_3.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 3 is invalid.");
            }
            else if(this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }

        }
        //NAND INPUT 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 4, out logic_4))
        {
            LogicNode logic_behavior = logic_4.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 4 is invalid.");
            }
            else if(this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }

        }
        //NAND ------OUTPUT------- 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 5, out logic_5))
        {
            LogicNode logic_behavior = logic_5.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            LogicNode lnode_0, lnode_1, lnode_3, lnode_4; //LogicNode references
            lnode_0 = logic_0.GetComponent<LogicNode>(); lnode_1 = logic_1.GetComponent<LogicNode>();
            lnode_3 = logic_3.GetComponent<LogicNode>(); lnode_4 = logic_4.GetComponent<LogicNode>();
            int low = (int)LOGIC.LOW;
            int invalid = (int)LOGIC.INVALID;
            if (IsDeviceOn())
            {
                if (lnode_0.GetLogicState() == low && lnode_1.GetLogicState() == low
                && lnode_3.GetLogicState() == low && lnode_4.GetLogicState() == low)
                {
                    logic_behavior.SetLogicState((int)LOGIC.HIGH);
                }
                else if (lnode_0.GetLogicState() != invalid && lnode_1.GetLogicState() != invalid
                && lnode_3.GetLogicState() != invalid && lnode_4.GetLogicState() != invalid)
                {
                    logic_behavior.SetLogicState((int)LOGIC.LOW);
                }
                else
                {
                    logic_behavior.SetLogicState((int)LOGIC.INVALID);
                }
            }
            else
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
            }
            collided_behavior.RequestStateChange(logic_behavior.GetLogicState());
        }
        //NAND INPUT 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 8, out logic_8))
        {
            LogicNode logic_behavior = logic_8.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 8 is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND INPUT 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 9, out logic_9))
        {
            LogicNode logic_behavior = logic_9.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 9 is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND INPUT 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 11, out logic_11))
        {
            LogicNode logic_behavior = logic_11.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 11 input is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND INPUT 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 12, out logic_12))
        {
            LogicNode logic_behavior = logic_12.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("NAND 74LS400 input 12 is invalid.");
            }
            else if (this.IsDeviceOn())
            {
                logic_behavior.SetLogicState(collided_state);
            }
        }
        //NAND ------OUTPUT----- 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 7, out logic_7))
        {
            LogicNode logic_behavior = logic_7.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            LogicNode lnode_8, lnode_9, lnode_11, lnode_12; //LogicNode references
            lnode_8 = logic_8.GetComponent<LogicNode>(); lnode_9 = logic_9.GetComponent<LogicNode>();
            lnode_11 = logic_11.GetComponent<LogicNode>(); lnode_12 = logic_12.GetComponent<LogicNode>();
            int low = (int)LOGIC.LOW;
            int invalid = (int)LOGIC.INVALID;
            if (IsDeviceOn())
            {
                if (lnode_8.GetLogicState() == low && lnode_9.GetLogicState() == low
                && lnode_11.GetLogicState() == low && lnode_12.GetLogicState() == low)
                {
                    logic_behavior.SetLogicState((int)LOGIC.HIGH);
                    
                }
                else if (lnode_8.GetLogicState() != invalid && lnode_9.GetLogicState() != invalid
                && lnode_11.GetLogicState() != invalid && lnode_12.GetLogicState() != invalid)
                {
                    logic_behavior.SetLogicState((int)LOGIC.LOW);
                }
                else
                {
                    logic_behavior.SetLogicState((int)LOGIC.INVALID);
                }
            }
            else
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
            }
            collided_behavior.RequestStateChange(logic_behavior.GetLogicState());
        }
    }
    private void CheckIfSnapped()
    {
        Debug.Log("74LS00 Mouse Up");

        //Check if all nodes with the chip is colliding with another logic node;
        foreach (KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logic_node = entry.Value;
            LogicNode logic_behavior = logic_node.GetComponent<LogicNode>();
            if (logic_behavior.GetCollidingNode() == null)
            {
                //indicator
                SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
                spr_ren.material.color = new Color(1, 1, 1); //neutral
                SNAPPED = false;
                Debug.Log("Snap not set.");
                return;
            }
        }
        //On release of mouse, SNAP the chip to the position
        GameObject node_left;
        //get both top left and top right logic nodes on the chip to check if they collided with any other logic nodes
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 0, out node_left))
        {
            LogicNode logicNodeScript_l = node_left.GetComponent<LogicNode>();
            GameObject collidingNodeLeft = logicNodeScript_l.GetCollidingNode();
            Debug.Log("74LS00 SNAPPED!");
            Vector3 collidingNodePos = collidingNodeLeft.transform.position;
            Vector3 offsetPosition = new Vector3(collidingNodePos.x + .245f, collidingNodePos.y - .58f, collidingNodePos.z);
            DeviceGameObject.transform.position = offsetPosition;
            //indicator
            SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
            spr_ren.material.color = new Color(0, 1, 0); //green
            SNAPPED = true;
        }
    }
    void OnMouseUp()
    {
        CheckIfSnapped();
    }

     
    
    private void ClearChip()
    {
        foreach(KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logicNodeGameObj = entry.Value;
            LogicNode logic_node = logicNodeGameObj.GetComponent<LogicNode>();
            logic_node.SetLogicState((int)LOGIC.INVALID);
        }
    }

    // Update is called once per frame
    void Update () {

	}

    public void ReactToLogic(GameObject logicNode, int requestedState)
    {
        if(requestedState == (int)LOGIC.INVALID && !SNAPPED)
        {
            ClearChip();
        }
        //Check if chip is snapped to protoboard, and then updates logic
        else if (SNAPPED)
        {
            ChipIO();
        }

    }

    public void ReactToLogic(GameObject LogicNode)
    {

    }
}
