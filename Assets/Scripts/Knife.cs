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
        if (other.GetComponent<Background>()) return; //����� �� ������ blackground

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

        //��������� �������� ������������, ������ ����� ������������ ��������� ������ �� ���
        //����� ������� ��������� ������ ��������� ������ ���������, ����� �� �� ����� �������� ����� �������
        int[] blendIndexs = paper.RemoveOldBlend(other.transform.parent.gameObject);

        //Slicer ��������� ��� �� ���������� Plane
        GameObject[] slices = Slicer.Slice(plane, other.gameObject);

        

        List<GameObject> pieces = new List<GameObject>();

        //������ ������ �� ������ ���� ������ �������, ��� ���������� �������� �������
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

        //��������� ������ �� ����� ������� �� �������� ��������� ���������
        foreach (var index in blendIndexs)
        {
            paper.UpdateBlend(index, pieces.ToArray());
        }

        //����� ������� ��������� ��� ���������� �������� ��������� �� ��������
        plane.Set3Points(_triggerEnterTipPosition, _triggerEnterBasePosition, _triggerExitTipPosition);

        //�������� �������, ���������� ����� �������� � ����� ������
        paper.UpdatePieces(other.transform.parent.gameObject, pieces.ToArray());
        //���������� ��������� ������� (��� ��������)
        paper.AddFlex(plane);

        //F
        Destroy(other.transform.parent.gameObject);
    }
}
