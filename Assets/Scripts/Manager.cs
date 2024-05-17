using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;

public class Manager : MonoBehaviour
{
    public HashSet<Vector2> liveCells = new HashSet<Vector2>();

    private int genCount;

    public Tile cellPrefab;
    [SerializeField] private Tilemap tilemap;

    [SerializeField] int tps = 3;

    private bool running;

    [SerializeField] private string LoadablePresetStr;

    private string saveString = "";

    // Start is called before the first frame update
    private void Start()
    {
        LoadPreset(LoadablePresetStr);
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            running = !running;
            if (running)
            {
                InvokeRepeating("tick", 0, 1f / tps);
            }
            else
            {
                CancelInvoke("tick");
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            running = false;

            CancelInvoke("tick");

            Render(new HashSet<Vector2>());

            liveCells.Clear();

            LoadPreset(LoadablePresetStr);

            Render(liveCells);

        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            running = false;

            CancelInvoke("tick");

            SaveCurrentState();

            print(saveString);

        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            running = false;

            CancelInvoke("tick");

            SaveCurrentState();

            print(saveString);

            Render(new HashSet<Vector2>());

            liveCells.Clear();

            LoadPreset(LoadablePresetStr);

            Render(liveCells);
        }

    }


    private void tick()
    {
        print("Tick Started: " + genCount.ToString());

        genCount++;

        CheckLiveCells();


        print("Tick Complete: " + genCount.ToString());
    }

    private void CheckLiveCells()
    {
        int currentNeighbours = 0;
        HashSet<Vector2> newLiveCells = new HashSet<Vector2>();
        HashSet<Vector2> deadCellsToCheck = new HashSet<Vector2>();

        foreach (Vector2 cell in liveCells)
        {
            currentNeighbours = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    Vector2 neighbour = new Vector2(cell.x + x, cell.y + y);

                    if (liveCells.Contains(neighbour))
                    {
                        currentNeighbours++;
                    }
                    else
                    {
                        deadCellsToCheck.Add(neighbour);
                    }
                }
            }
            if (currentNeighbours == 2 || currentNeighbours == 3)
            {
                newLiveCells.Add(cell);
            }
        }

        foreach (Vector2 deadCell in deadCellsToCheck)
        {
            CheckDeadCell(newLiveCells, deadCell);
        }

        Render(newLiveCells);

        liveCells = newLiveCells;
    }

    private void CheckDeadCell(HashSet<Vector2> newLiveCells, Vector2 pos)
    {
        int currentNeighbours = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                if (liveCells.Contains(new Vector2(pos.x + x, pos.y + y)))
                {
                    currentNeighbours++;
                }
            }
        }
        if (currentNeighbours == 3)
        {
            newLiveCells.Add(pos);
        }
    }

    private void Render(HashSet<Vector2> newLiveCells)
    {
        foreach (Vector2 cell in liveCells)
        {
            tilemap.SetTile(new Vector3Int((int)cell.x, (int)cell.y, 0), null);
        }

        foreach (Vector2 cell in newLiveCells)
        {
            tilemap.SetTile(new Vector3Int((int)cell.x, (int)cell.y, 0), cellPrefab);
        }
    }

    private void LoadPreset(string presetStr)
    {

        string currentNumber = "";
        int xValue = 0;
        int yValue = 0;

        for (int i = 0; i < presetStr.Length; i++)
        {
            if (presetStr[i] == 'o' || presetStr[i] == 'b' || presetStr[i] == '$')
            {
                currentNumber = "";
                for (int j = 1; j < i+1; j++)
                {
                    if (presetStr[i-j] == 'b' || presetStr[i-j] == '$' || presetStr[i-j] == 'o')
                    {
                        break;
                    }
                    currentNumber = presetStr[i-j].ToString() + currentNumber;
                }

                currentNumber = currentNumber.Trim();

                if (presetStr[i] == 'o')
                {
                    print("Attempting to convert to cell : " + currentNumber);
                    if(currentNumber == "")
                    {
                        liveCells.Add(new Vector2(xValue, yValue));
                        xValue++;
                        
                    }
                    else
                    {
                        for (int j = 0; j < Convert.ToInt32(currentNumber); j++)
                        {
                            liveCells.Add(new Vector2(xValue + j, yValue));
                        }
                        xValue += Convert.ToInt32(currentNumber);

                    }
                }
                if (presetStr[i] == 'b')
                {
                    print("converting to x : " + currentNumber);

                    if(currentNumber == "")
                    {
                        xValue++;
                    }
                    else
                    {
                        xValue += Convert.ToInt32(currentNumber);
                    }
                }
                if (presetStr[i] == '$')
                {
                    print("converting to y : " + currentNumber);

                    if(currentNumber == "")
                    {
                        yValue++;
                        xValue = 0;
                    }
                    else
                    {
                        yValue += Convert.ToInt32(currentNumber);
                        xValue = 0;
                    }
                }   

            }

        }

        Render(liveCells);
    }

    private void SaveCurrentState()
    {
        int maxY = (int)liveCells.Max(v => v.y);
        int minY = (int)liveCells.Min(v => v.y);
        int maxX = (int)liveCells.Max(v => v.x);
        int minX = (int)liveCells.Min(v => v.x);

        int newLineCount = 0;

        int prevCellX = minX;
        int currentConcurantLiveCells = 0;

        HashSet<Vector2>[] rows = new HashSet<Vector2>[maxY - minY - 1];

        foreach (Vector2 cell in liveCells)
        {

            rows[(int)cell.y - minY - 1].Add(cell);

        }

        for ( int i = 0; i < rows.Length; i++)
        {
            rows[i] = rows[i].OrderBy(v => v.x).ToHashSet();
        }

        foreach (HashSet<Vector2> row in rows)
        {
            if (row.Count == 0)
            {
                newLineCount++;

            }
            else if ( row != rows[0] )
            {
                saveString += newLineCount.ToString() + "$";
            }

            foreach (Vector2 cell in row)
            {
                if (row != rows[0] && cell != row.First())
                {
                    if (cell.x - prevCellX > 1)
                    {
                        saveString += (cell.x - prevCellX).ToString() + "b";
                        if (currentConcurantLiveCells == 0)
                        {
                            saveString += "o";
                        }
                        else
                        {
                            saveString += currentConcurantLiveCells.ToString() + "o";
                        }
                        currentConcurantLiveCells = 0;
                    }
                    else
                    {
                        currentConcurantLiveCells++;
                    }
                }
                
            }
            if (row.Last().x != maxX)
            {
                saveString += (maxX - row.Last().x).ToString() + "b";
            }
        }

    }
}
