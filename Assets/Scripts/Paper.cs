using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paper : MonoBehaviour
{
    [SerializeField] Joystick joystick;

    public  List<GameObject> pieces = new List<GameObject>();
    public List<Plane> flexs = new List<Plane>();
    public List<Blend> blends = new List<Blend>();
    Blend curretBlend;
    Plane currentFlex;
    float currentAngle;
    float angle;
    GameObject rotatePivot;
    public bool blendActive = false;
    private void Awake()
    {
    }
    private void Update()
    {
        Blend();
    }
    #region Update
    //ѕоскольку сгиб может быть в любую сторону, после спавна rotatePivot разворачиваетс€ по нормали сгиба,
    //теперь просто при повороте просто наклон€етс€ на 180 по оси X
    private void Blend()
    {
        if (blendActive)
        {

            if (Mathf.Abs(currentAngle) < 180)
            {
                //привороте angle - положительный, при возврате - отрицальный
                float addAngle = angle; // *speed * deltaTime при желании можно доавбить скорость и врем€
                rotatePivot.transform.rotation *= Quaternion.Euler(new Vector3(-addAngle, 0,0));
                currentAngle += addAngle; 
            }
            else
            {
                foreach (var piece in pieces)
                {
                    piece.transform.SetParent(transform);
                }
                //Destroy(rotatePivot); //позже rotatePivot используетс€ дл€ возврата сгиба
                blendActive = false;
            }
        }
    }
    #endregion

    #region Player
    public void PieceSelect(Vector3 point) //Player Update
    {
        if (!blendActive && joystick.Direction.magnitude > 0.2f)
        {
            SetCurrentFlex(point);

            SetRotatePivot(point);

            SetRotationPieces(point);

            blends.Add(curretBlend);
            currentAngle = 0;
            angle = 1f;
            blendActive = true;
        }
    }

    //¬ыбор ближайшего в направлении поворота сгиба
    private void SetCurrentFlex(Vector3 point)
    {
        Vector3 directionRay = new Vector3(joystick.Direction.x, 0, joystick.Direction.y);
        Ray ray = new Ray(point, directionRay);
        float distance = float.MaxValue;
        foreach (var flex in flexs)
        {
            float newDistance = 0;
            if (flex.Raycast(ray, out newDistance))
            {
                if (Mathf.Abs(newDistance) < distance)
                {
                    distance = Mathf.Abs(newDistance);
                    currentFlex = flex;
                }
            }
        }
    }
    //установка объекта, относительно котрого произойдЄт вращение
    private void SetRotatePivot(Vector3 point)
    {
        rotatePivot = new GameObject("rotatePivot");
        rotatePivot.transform.position = currentFlex.ClosestPointOnPlane(point);
        rotatePivot.transform.LookAt(point);

        curretBlend = new Blend(rotatePivot);
    }
    //опредление кусочков которые необходимо повернуть 
    private void SetRotationPieces(Vector3 point)
    {
        bool positive;
        if (currentFlex.GetDistanceToPoint(point) > 0)
            positive = true;
        else
            positive = false;

        foreach (var piece in pieces)
        {
            if ((currentFlex.GetDistanceToPoint(piece.transform.position) > 0 && positive)
                || (currentFlex.GetDistanceToPoint(piece.transform.position) < 0 && !positive))
            {
                piece.transform.SetParent(rotatePivot.transform);

                curretBlend.pieces.Add(piece);
            }
        }
        rotatePivot.transform.position += new Vector3(0, 0.1f, 0);
    }
    #endregion

    #region Knife
    //удаление старого, добавление новых кусочков в общий массив после разреза
    public void UpdatePieces(GameObject delete, GameObject[] add)
    {
        if (pieces.Contains(delete))
            pieces.Remove(delete);
        pieces.AddRange(add);
        foreach (var piece in add)
        {
            piece.transform.SetParent(transform);
        }
    }
    //добавление сгиба
    public void AddFlex(Plane plane)
    {
        flexs.Add(plane);
    }
    //”даление ссылок на кусочки, которые будут уничтожены после разреза
    public int[] RemoveOldBlend(GameObject piece)
    {
        List<int> indexes = new List<int>();
        foreach (var blend in blends)
        {
            if (blend.pieces.Contains(piece))
            {
                blend.pieces.Remove(piece);
                indexes.Add(blends.IndexOf(blend));
            }
        }
        return indexes.ToArray();
    }
    //ƒобавление новых кусочков в Blend из которых были удаены старые
    public void UpdateBlend(int blendIndex, GameObject[] pieces)
    {
        blends[blendIndex].pieces.AddRange(pieces);
    }
    #endregion

    #region Button
    //возврашение последнего поворота
    public void BackPiece() //button Back
    {
        if (!blendActive)
        {
            Blend blend = blends [blends.Count - 1];
            blends.Remove(blend);

            rotatePivot = blend.pivotRotate;
            if (rotatePivot == null)
                return;

            foreach (var piece in blend.pieces)
            {
                piece.transform.SetParent(rotatePivot.transform);
            }
            rotatePivot.transform.position += new Vector3(0, -0.1f, 0);

            currentAngle = 0;
            angle = -1f;
            blendActive = true;
        }
    }
    #endregion
}

// ¬ этой структуре сохранетс€ все дл€ отката поворота (центр поворота и повернутые кусочки)
public struct Blend
{
    public Blend(GameObject pivot)
    {
        pieces = new List<GameObject>();
        pivotRotate = pivot;
    }
    public List<GameObject> pieces;
    public GameObject pivotRotate;
}
