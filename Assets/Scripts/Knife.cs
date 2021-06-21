using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    [SerializeField] GameObject _tip = null;
    [SerializeField] GameObject _base = null;

    private Vector3 _triggerEnterTipPosition;
    private Vector3 _triggerEnterBasePosition;
    private Vector3 _triggerExitTipPosition;
    Vector3 pointEnter;
    Vector3 pointExit;

    Paper paper;
    private void Awake()
    {
        paper = FindObjectOfType<Paper>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Background>()) return; //Чтобы не резать blackground

        _triggerEnterTipPosition = _tip.transform.position;
        _triggerEnterBasePosition = _base.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Background>()) return;

        #region CreatePlane
        _triggerExitTipPosition = _tip.transform.position;

        Vector3 side1 = _triggerExitTipPosition - _triggerEnterTipPosition;
        Vector3 side2 = _triggerExitTipPosition - _triggerEnterBasePosition;

        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

        Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(_triggerEnterBasePosition);

        Plane plane = new Plane();

        plane.SetNormalAndPosition(
                transformedNormal,
                transformedStartingPoint);

        var direction = Vector3.Dot(Vector3.up, transformedNormal);

        if (direction < 0)
        {
            plane = plane.flipped;
        }

        #endregion

        //Разренные кусочеки уничтожаются, потому перед уничтожением удаляется ссылка на них
        //Также функция удаляющая ссылку возвращет индекс удаленных, чтобы на их месте добавить новые кусочки
        int[] blendIndexs = paper.RemoveOldBlend(other.transform.parent.gameObject);

        //Slicer разрезает мэш по указанному Plane
        GameObject[] slices = Slicer.Slice(plane, other.gameObject);

        

        List<GameObject> pieces = new List<GameObject>();

        //Создаём объект по центру мэша нового кусочка, для коректного постчёта позиции
        foreach (var obj in slices)
        {
            GameObject newPiece = new GameObject("Piece");
            Vector3[] vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
            Vector3 total = new Vector3(0,0,0);
            foreach (var ver in vertices)
            {
                //print(ver);
                total += obj.transform.TransformPoint(ver);
            }
            if (vertices.Length > 0)
            {
                Vector3 position = total / vertices.Length;
                newPiece.transform.position = position;
            }
            
            //newPiece.transform.position = obj.GetComponent<Renderer>().bounds.center;
            obj.transform.SetParent(newPiece.transform);
            pieces.Add(newPiece);
        }

        //Добавляет ссылку на новые кусочки по индекнсу удалённого старового
        foreach (var index in blendIndexs)
        {
            paper.UpdateBlend(index, pieces.ToArray());
        }

        //Смена позиции плоскости для коректного просчета растояния до кусочков
        plane.Set3Points(_triggerEnterTipPosition, _triggerEnterBasePosition, _triggerExitTipPosition);

        //удаление старого, добавление новых кусочков в общий массив
        paper.UpdatePieces(other.transform.parent.gameObject, pieces.ToArray());
        //сохранение плоскости разреза (для поворото)
        paper.AddFlex(plane);

        //F
        Destroy(other.transform.parent.gameObject);
    }
}
