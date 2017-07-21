using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Checkerboard : MonoBehaviour {
    
    public int boardWidth = 6;
    public int boardDepth = 7;

    private bool _instantiateDark = false;

    public GameObject boardDarkPiece;
    public GameObject boardLightPiece;

    public GameObject blueCube;
    public GameObject greenCube;
    public GameObject pinkCube;
    public GameObject redCube;
    public GameObject yellowCube;

    private Highlight _lastHighlighted = null;
    private GameObject _currentlyHighlighted = null;

    public GameObject[,] boardMap;



    public GameObject selectedObject = null;


    public Material colorBlindMaterial;
    public Material colorBlindHighlightMaterial;
    public bool colorBlind = false;

    private bool _init = false;

    public static float moveUp = 0.5f;
    public float scaleFactor = 0.03f;

    public void Init()
    {
        if (colorBlind)
        {
            blueCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial);
            greenCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial);
            pinkCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial);
            redCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial);
            yellowCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial);
        }


        boardMap = new GameObject[boardWidth, boardDepth];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardDepth; j++)
            {
                if (i >= boardWidth) i = 0;
                if (j >= boardDepth) j = 0;
                GameObject go = Instantiate(_instantiateDark ? boardDarkPiece : boardLightPiece, transform); // hehe isto só funciona com boardDepth impares :D
                boardMap[i, j] = go;
                go.name = "box(" + i + "," + j + ")";
                _instantiateDark = !_instantiateDark;
                go.transform.position = new Vector3(j - boardDepth / 2, 0, i - boardWidth / 2);
            }
        }

        moveUp = blueCube.transform.localScale.y / 2f;

        

        GameObject negativeSpaceCenter = GameObject.Find("NegativeSpaceCenter");
        NegativeSpace negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();

        transform.position = negativeSpace.bottomCenterPosition + 0.001f * negativeSpaceCenter.transform.up;
        transform.rotation = negativeSpaceCenter.transform.rotation;

        moveUp = moveUp * scaleFactor;
        Debug.Log(this.ToString() + ": Scaling.");
        transform.localScale = transform.localScale * scaleFactor;

        string path = Application.dataPath + "/initPositions.txt";
        if (System.IO.File.Exists(path))
        {
            String[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.Contains(" ontopof "))
                {
                    string[] s = line.Split(' ');
                    putObjectOnTopOf(GameObject.Find(s[0]), GameObject.Find(s[2]));
                }
            }
        }
        else
        {
            putObjectOnTopOf(blueCube, 0, 0);
            putObjectOnTopOf(greenCube, 1, 0);
            putObjectOnTopOf(pinkCube, 2, 0);
            putObjectOnTopOf(redCube, 3, 0);
            putObjectOnTopOf(yellowCube, 4, 0);
        }

        _init = true;
    }


    void Update()
    {
        /*
        if (!_init) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            IAmPointing(ray, true);
        }
        else
        {
            IAmPointing(ray, false);
        }
        */
    }

    public void IAmPointing(Ray ray, bool click)
    {
        if (!_init) return;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            Highlight h = hit.transform.GetComponent<Highlight>();
            if (h != null)
            {
                Debug.Log(this.ToString() + "[IAmPointing]: " + h.name);
                h.setHighlight(true);
                if (_lastHighlighted != null && _lastHighlighted != h) _lastHighlighted.setHighlight(false);
                _lastHighlighted = h;
                _currentlyHighlighted = hit.transform.gameObject;
            }
        }
        else
        {
            if (_lastHighlighted != null) _lastHighlighted.setHighlight(false);
            _lastHighlighted = null;
        }

        if (click)
        {
            if (_currentlyHighlighted != null)
            {
                _selection(_currentlyHighlighted);
            }
        }
        _currentlyHighlighted = null;
    }

    private void _selection(GameObject o)
    {
        if (selectedObject != null) selectedObject.GetComponent<Highlight>().setSelected(false);




        if (selectedObject == null && isCube(o))
        {
            selectedObject = o;
            selectedObject.GetComponent<Highlight>().setSelected(true);
        }
        else if (selectedObject != null)
        {
            selectedObject.GetComponent<Highlight>().setSelected(false);
            putObjectOnTopOf(selectedObject, o);
            selectedObject = null;
        }
    }

    void putObjectOnTopOf(GameObject that, int thereWidth, int thereDepth)
    {
        if (thereWidth <= boardMap.GetLength(0) && thereWidth >= 0
            && thereDepth <= boardMap.GetLength(1) && thereDepth >= 0)
        {
            putObjectOnTopOf(that, boardMap[thereWidth, thereDepth]);
        }
    }

    void putObjectOnTopOf(GameObject that, GameObject there)
    {

        if (that == there) return;

        float d = moveUp;
        if (isCube(there))
        {
            d = 2*d;
        }

        that.GetComponent<CubeBehaviour>().onTopOf = there;
        that.GetComponent<CubeBehaviour>().GoTo(there.transform.position + d * (GameObject.Find("NegativeSpaceCenter").transform.up));
        
        //Debug.Log(createInstruction(that, there));
    }

    public static bool isCube(GameObject v)
    {
        return v.name.EndsWith("Cube");
    }

    private bool isBoard(GameObject v)
    {
        return v.name.StartsWith("Box");
    }

    public string createInstruction(GameObject that, GameObject there)
    {
        if (!isCube(there))
        {
            string s = there.name.TrimStart("box(".ToCharArray());
            s = s.TrimEnd(')');
            string[] values = s.Split(',');

            return that.name + " " + values[0] + " " + values[1];
        }
        else return that.name + " " + there.name;
    }

    void __OnGUI()
    {
        int width = Screen.width / 10;
        int height = width / 2;
        int bezel = 10;

        Rect interactionArea = new Rect(Screen.width - width, 0, width, height);
        if (Input.mousePosition.x > interactionArea.x && Screen.height - Input.mousePosition.y < interactionArea.height)
        {
            GUI.Box(interactionArea, "");
            if (GUI.Button(new Rect(interactionArea.x + bezel, interactionArea.y + bezel, interactionArea.width - 2 * bezel, interactionArea.height - 2 * bezel), "Save"))
            {
                CubePositionsFileWriter f = new CubePositionsFileWriter();
                f.addLine(redCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(greenCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(blueCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(yellowCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(pinkCube.GetComponent<CubeBehaviour>().initPosition());

                bool saved;
                f.tryFlush(Application.dataPath + "/initPositions.txt", out saved);
                if (saved) Debug.Log("init positions saved");
            }
        }
    }
}
