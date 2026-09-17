using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coding : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log( v_GetCode("1234567890",true,"0450"));
        }
    }

    public string v_GetCode(string i_iduser, bool i_Win, string i_time)
    {
         
        List<string> l_listTemp = new List<string>();
        for (int i=0;i<20;i++)
        {
            l_listTemp.Add("*");
        }

        if (i_iduser.Length != 10)
        {
            
            Debug.Log("problem Length id user ");
            return "";
        }
        if (i_time.Length != 4)
        {
            Debug.Log("problem Length time ");
            return "";
        }



        if (i_Win)
        {
            l_listTemp[5] = "5";
            l_listTemp[9] = "3";
            l_listTemp[14] = "4";
        }
        else
        {
            l_listTemp[5] = "8";
            l_listTemp[9] = "4";
            l_listTemp[14] = "1";
        }

        int l_random = Random.Range(1, 4);
        l_listTemp[8] = l_random.ToString();
        l_listTemp[10] = (l_random * 2).ToString();


        //id user 
        l_listTemp[17] = i_iduser[0].ToString();
        l_listTemp[15] = i_iduser[1].ToString();
        l_listTemp[7] = i_iduser[2].ToString();
        l_listTemp[11] = i_iduser[3].ToString();
        l_listTemp[13] = i_iduser[4].ToString();
        l_listTemp[4] = i_iduser[5].ToString();
        l_listTemp[2] = i_iduser[6].ToString();
        l_listTemp[12] = i_iduser[7].ToString();
        l_listTemp[3] = i_iduser[8].ToString();
        l_listTemp[19] = i_iduser[9].ToString();
        //////////

        //time
        l_listTemp[1] = i_time[0].ToString();
        l_listTemp[6] = i_time[1].ToString();
        l_listTemp[16] = i_time[2].ToString();
        l_listTemp[18] = i_time[3].ToString();
        ///////////


        int l_random2 = Random.Range(0, 9);
        l_listTemp[0] = l_listTemp.ToString();
        if (l_random2 == 0)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "1";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 1)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "2";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 2)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "0";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 3)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "7";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 4)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "4";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 5)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "5";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 6)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "0";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 7)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "6";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 8)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "7";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "2";
                }
                //////////////////////////////////////////////
            }
        }
        else if (l_random2 == 9)
        {
            for (int i = 1; i <= 19; i++)
            {
                if (l_listTemp[i] == "0")
                {
                    l_listTemp[i] = "5";
                }
                else if (l_listTemp[i] == "1")
                {
                    l_listTemp[i] = "6";
                }
                else if (l_listTemp[i] == "2")
                {
                    l_listTemp[i] = "1";
                }
                else if (l_listTemp[i] == "3")
                {
                    l_listTemp[i] = "4";
                }
                else if (l_listTemp[i] == "4")
                {
                    l_listTemp[i] = "3";
                }
                else if (l_listTemp[i] == "5")
                {
                    l_listTemp[i] = "8";
                }
                else if (l_listTemp[i] == "6")
                {
                    l_listTemp[i] = "2";
                }
                else if (l_listTemp[i] == "7")
                {
                    l_listTemp[i] = "0";
                }
                else if (l_listTemp[i] == "8")
                {
                    l_listTemp[i] = "9";
                }
                else if (l_listTemp[i] == "9")
                {
                    l_listTemp[i] = "7";
                }
                //////////////////////////////////////////////
            }
        }






        string temp = "";
        for (int i = 0; i < l_listTemp.Count; i++)
        {
            temp = temp + l_listTemp[i];
        }
        return temp.ToString();
    }
}
