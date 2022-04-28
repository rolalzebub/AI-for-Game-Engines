using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{

    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int X;
        public int Z;
    }

    private int Width;
    private int Height;
    private float CellSize;
    private Vector3 OriginPosition;

    private TGridObject[,] GridArray;

    public Grid(int Width, int Height, float CellSize, Vector3 OriginPosition, Func<Grid<TGridObject>, int, int, TGridObject> CreateGridObject)
    {
        this.Width = Width;
        this.Height = Height;
        this.CellSize = CellSize;
        this.OriginPosition = OriginPosition;

        GridArray = new TGridObject[Width, Height];

        for (int x = 0; x < GridArray.GetLength(0); x++)
        {
            for (int z = 0; z < GridArray.GetLength(1); z++)
            {
                GridArray[x, z] = CreateGridObject(this, x, z);
            }
        }

        bool Debugshow = false;

        if (Debugshow)
        {
            TextMesh[,] DebugTextArray = new TextMesh[Width, Height];


            for (int x = 0; x < GridArray.GetLength(0); x++)
            {
                for (int z = 0; z < GridArray.GetLength(1); z++)
                {
                    DebugTextArray[x, z] = CreateWorldText(GridArray[x, z]?.ToString(), null, GetWorldPosition(x, z) + new Vector3(CellSize, 0, CellSize) * 0.5f, 10, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center, 1);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, Height), GetWorldPosition(Width, Height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(Width, 0), GetWorldPosition(Width, Height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                DebugTextArray[eventArgs.X, eventArgs.Z].text = GridArray[eventArgs.X, eventArgs.Z]?.ToString();
            };
        }

        

        
    }

    public int GetWidth()
    {
        return Width;
    }
    public int GetHeight()
    {
        return Height;
    }

    public float GetCellSize()
    {
        return CellSize;
    }

    private Vector3 GetWorldPosition(int X, int Z)
    {
        return new Vector3(X, 0, Z) * CellSize + OriginPosition;
    }
    public void GetXZ(Vector3 WorldPosition, out int X, out int Z)
    {
        //X = Mathf.FloorToInt((WorldPosition - OriginPosition).x / CellSize);
        X = Mathf.RoundToInt(Mathf.Abs(WorldPosition.x - OriginPosition.x / CellSize));
        Z = Mathf.RoundToInt(Mathf.Abs(WorldPosition.z - OriginPosition.z / CellSize));
        //Z = Mathf.FloorToInt((WorldPosition - OriginPosition).z / CellSize);
    }

    private void SetValue(int X, int Z, TGridObject Value)
    {
        if (X >= 0 && Z >= 0 && X < Width && Z < Height)
        {
            GridArray[X, Z] = Value;
            if (OnGridValueChanged != null)
            {
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { X = X, Z = Z });
            }
        }
    }

    public void TriggerGridChange(int X, int Z)
    {
        if (OnGridValueChanged != null)
        {
            OnGridValueChanged(this, new OnGridValueChangedEventArgs { X = X, Z = Z });
        }
    }

    public void SetValue(Vector3 WorldPosition, TGridObject Value)
    {
        int X;
        int Z;
        GetXZ(WorldPosition, out X, out Z);
        SetValue(X, Z, Value);

    }

    public TGridObject GetValue(int X, int Z)
    {
        if (X >= 0 && Z >= 0 && X < Width && Z < Height)
        {
            return GridArray[X, Z];
        } else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetValue(Vector3 WorldPosition)
    {
        int X;
        int Z;
        GetXZ(WorldPosition, out X, out Z);
        return GetValue(X, Z);
    }


    public static TextMesh CreateWorldText(string Text, Transform Parent = null, Vector3 LocalPosition = default(Vector3), int FontSize = 1, Color? color = null, TextAnchor TextAnchor = TextAnchor.UpperLeft, TextAlignment TextAlignment = TextAlignment.Left, int SortingOrder = 5000 )
    {
        if (color == null)
        {
            color = Color.white;
        }
        return CreateWorldText(Parent, Text, LocalPosition, FontSize, (Color)color, TextAnchor, TextAlignment, SortingOrder);
    }

    public static TextMesh CreateWorldText(Transform Parent, string Text, Vector3 LocalPosition, int FontSize, Color Color, TextAnchor TextAnchor, TextAlignment TextAllignment, int SortingOrder)
    {
        GameObject GameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform Transform = GameObject.transform;
        Transform.SetParent(Parent, false);
        Transform.localPosition = LocalPosition;
        TextMesh TextMesh = GameObject.GetComponent<TextMesh>();
        TextMesh.anchor = TextAnchor;
        TextMesh.alignment = TextAllignment;
        TextMesh.text = Text;
        TextMesh.fontSize = FontSize;
        TextMesh.color = Color;
        TextMesh.GetComponent<MeshRenderer>().sortingOrder = SortingOrder;
        return TextMesh;

    }



}
