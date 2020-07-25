using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuminousManage : MonoBehaviour
{
    // Beam source Material
    public GameObject HandPoint;
    // Beam Material
    public GameObject Beem;
    
    // ビームを発射するまでの時間
    private float charge_time = 1;

    // 光り具合を管理する定数
    private float Facter = 0;
    private Color _EmissionColor;
    private Renderer _renderer;
    
    /* ビームの状態の定数 
    -1: 何もしていない状態
    0 : charge状態
    1 : 発射している        */
    private int _beemStatus = -1;
    
    // ビームの発射点とビーム
    private GameObject newBeemSource;
    private GameObject newBeem;
    

    public int Attack(float ration, Vector3 BeemSourcePosition, Vector3 AttakDirection)
    {
        // BeemSourceを作る
        if (Facter == 0)
        {
            newBeemSource = Instantiate(HandPoint, BeemSourcePosition, Quaternion.identity) as GameObject;
            _renderer = newBeemSource.GetComponent<Renderer>();
            _renderer.material.EnableKeyword("_EMISSION");
            _EmissionColor = _renderer.material.GetColor("_EmissionColor");
        }

        Facter += ration;
        // charge状態
        if (Facter <= charge_time)
        {
            _beemStatus = 0;
            _renderer.material.SetColor("_EmissionColor", new Color(_EmissionColor.r * (Facter + 1),
                _EmissionColor.g * (Facter + 1), _EmissionColor.b * (Facter + 1)));
            newBeemSource.transform.position = BeemSourcePosition;
            return 0;
        }
        // Beem発射
        else
        {
            if (_beemStatus == 0)
            {
                // Beemオブジェクトを生成
                newBeem = Instantiate(Beem, newBeemSource.transform.position, Quaternion.identity) as GameObject;
                
                // Beem発射状態になる
                _beemStatus = 1;
            }
            else
            {
                // Beemの発射位置とBeemの位置を更新
                newBeemSource.transform.position = BeemSourcePosition;
                newBeem.transform.position = newBeemSource.transform.position;
            }
            // Beemの方向を決定する
            Vector3 look = BeemSourcePosition + AttakDirection;
            look.z = look.z * -1;
            newBeem.transform.LookAt(look);
            return 1;
            
        }
    }

    private float RadToDeg(float rad)
    {
        // ラジアンを度数表記に変える
        return (float)(rad * 180 / Mathf.PI);
    }

    public void StopBeem()
    {
        // ビームを止める
        if (_beemStatus >= 0)
        {
            _renderer.material.SetColor("_EmissionColor", new Color(_EmissionColor.r, _EmissionColor.g, _EmissionColor.b));
            Facter = 0;
            _beemStatus = -1;
            Destroy(newBeem, 0.1f);
            Destroy(newBeemSource, 0.1f);
        }
        
    }
}
