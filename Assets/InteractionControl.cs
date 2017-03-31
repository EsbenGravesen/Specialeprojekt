using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionControl : MonoBehaviour {
    [SerializeField]
    private float shootCooldown;
    [SerializeField]
    int shootDistance = 100;
    [Header("Crosshair settings:")]
    [SerializeField]
    private Color DefaultColor;
    [SerializeField]
    private Color InRangeColor;
    [SerializeField]
    float normalRadius, bigRadius, smallRadius;
    [SerializeField]
    private AnimationCurve sizeChangeCurve;

    private float cd = 0, dNormBig, dBigSmall, dSmallNorm;
    private int hitLayer;
    private RaycastHit hitter;
    SpriteRenderer[] mySprites;
	// Use this for initialization
	void Start () {
        hitLayer = 1 << LayerMask.NameToLayer("Hitable");
        mySprites = new SpriteRenderer[2];
        mySprites[0] = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mySprites[1] = transform.GetChild(1).GetComponent<SpriteRenderer>();
        dNormBig = bigRadius - normalRadius;
        dBigSmall = smallRadius - bigRadius;
        dSmallNorm = normalRadius - smallRadius;
        setColor(DefaultColor);
        setSize(normalRadius);
	}
	private void setColor(Color col)
    {
        mySprites[0].color = col;
        mySprites[1].color = col;
    }
    private void setSize(float radius)
    {
        Vector3 s = new Vector3(radius, radius, 1);
        mySprites[0].transform.localScale = s;
        mySprites[1].transform.localScale = s;
    }
	// Update is called once per frame
	void Update () {
        cd -= Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && cd<=0)
        {
            cd = shootCooldown;
            Debug.Log("SHOOT");
            RaycastHit hitted;
        
            if (Physics.Raycast(transform.position, transform.forward, out hitted, shootDistance, hitLayer))
            {
                if (hitted.transform.gameObject.GetComponent<Stationary>() != null)
                {
                    Debug.Log(hitted.transform.gameObject.GetComponent<Stationary>().Activate());
                }
            }
            StartCoroutine(doShootPulse());
        }
        if(Physics.Raycast(transform.position, transform.forward,out hitter, shootDistance, hitLayer) && cd<=0)
        {
            setColor(InRangeColor);
        } 
        else
        {
            setColor(DefaultColor);
        }
    }
    private IEnumerator doShootPulse()
    {
        float normTime = 0;
        setSize(bigRadius);
        while (normTime <= 1)
        {
            if (normTime < 0.95f) //Big to small
            {
                setSize(bigRadius + dBigSmall * sizeChangeCurve.Evaluate(normTime / 0.95f));
            }
            else //Small to normal
            {
                setSize(smallRadius + dSmallNorm * sizeChangeCurve.Evaluate((normTime - 0.95f) / 0.05f));
            }
            normTime = 1f - cd / shootCooldown;
            yield return null;
        }
        setSize(normalRadius);
        yield break;
    }
}
