using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacterController : MonoBehaviour
{
    public GameObject Reacter;
    private GameObject newReacter;

    // 光り具合を管理する定数
    private float Facter = 0;
    private Color _EmissionColor;
    private Renderer _renderer;

    // ビームを発射するまでの時間
    private float charge_time = 1;

    //
    private float scaleConst = 5.0f;
    private List<float> HightList = new List<float>();
    public int average_num = 10;

    // Start is called before the first frame update
    void Start()
    {
        // リアクターオブジェクトを生成する
        newReacter = Instantiate(Reacter, new Vector3(0,0,0), Quaternion.identity) as GameObject;

        // レンダラーと色の基準を取得する
        _renderer = newReacter.GetComponentInChildren<Renderer>();
        _renderer.material.EnableKeyword("_EMISSION");
        _EmissionColor = _renderer.material.GetColor("_EmissionColor");
        newReacter.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }
    
    public void UpdateScale(float Height)
    {
        HightList.Add(Height);
        if (HightList.Count >= average_num)
        {
            float val = AverageList(HightList);
            float sc = scaleConst * val;
            newReacter.transform.localScale = new Vector3(sc, sc, sc);
            HightList.RemoveAt(0);
            Debug.Log(sc);
        }
    }


    // 
    public void UpdatePosition(Vector3 ReacterPosition, Vector3 ReactoerDirection)
    {
        newReacter.transform.position = ReacterPosition;
        Vector3 look = ReacterPosition + ReactoerDirection;
        look.z = look.z * -1;
        newReacter.transform.LookAt(look);
        newReacter.transform.Rotate(transform.forward, 180);
        newReacter.transform.Rotate(transform.right, 90);
    }

    public void Activate(float ration)
    {
        if (Facter <= charge_time)
        {
            Facter += ration;
            _renderer.material.SetColor("_EmissionColor", new Color(_EmissionColor.r * (Facter + 1),
                _EmissionColor.g * (Facter + 1), _EmissionColor.b * (Facter + 1)));
        }
    }

    public void DeActivate(float ration)
    {
        if (Facter >= 0)
        {
            Facter -= ration;
        }
        else
        {
            Facter = 0;
        }
        _renderer.material.SetColor("_EmissionColor", new Color(_EmissionColor.r * (Facter + 1),
                _EmissionColor.g * (Facter + 1), _EmissionColor.b * (Facter + 1)));
    }

    private float AverageList(List<float> array)
    {
        float all = 0;
        for (int i = 0; i < array.Count; i++)
        {
            all += array[i];
        }
        float ave = all / array.Count;
        return ave;
    }
}
