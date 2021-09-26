using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerLightning : MonoBehaviour
{
    [SerializeField] private GameObject lightningRodPrefab;
    [SerializeField] private GameObject lightningAttackPrefab;
    [SerializeField] private LightningStrike lightningSfx;
    [SerializeField] private Camera myCamera;
    [SerializeField] private Animator myAnimator;
    [SerializeField] private float lightningLifeTime = 1f;
    private List<GameObject> lightningRodsSummoned;
    private List<GameObject> lightningSummoned;
    private bool canPlaceRod = true;
    private float lightningCooldownLeft;
    private bool canAttack = true;

    public Camera MyCamera
    {
        get
        {
            if (!myCamera)
                myCamera = FindObjectOfType<Camera>();
            return myCamera;
        }
    }

    void Start()
    {
        lightningRodsSummoned = new List<GameObject>();
        lightningSummoned = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(canPlaceRod)
            {
                PlaceRod();
            }
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            ResetRods();
        }
        else if(Input.GetMouseButtonDown(1))
        {
            LightningAttack();
        }
    }

    private void PlaceRod()
    {
        if(lightningRodsSummoned.Count == PlayerStats.Current.MaxRods - 1)
        {
            canPlaceRod = false;
        }
        Ray ray = MyCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 rodSpawnPosition;
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            rodSpawnPosition = hit.point;
            rodSpawnPosition.y += 1f;
            GameObject newRod = GameObject.Instantiate(lightningRodPrefab, rodSpawnPosition, Quaternion.identity);
            lightningRodsSummoned.Add(newRod);
        }
    }

    private void ResetRods()
    {
        foreach(GameObject rod in lightningRodsSummoned)
        {
            Destroy(rod);
        }
        lightningRodsSummoned.Clear();
        canPlaceRod = true;

    }

    private void LightningAttack()
    {
        if(!canAttack || lightningRodsSummoned.Count < 1)
        {
            return;
        }
        myAnimator.SetBool("goAttack", true);
        Debug.Log("lightning animation");
        canAttack = false;
        StartCoroutine(AttackCooldown());
        for(int rodNum = 0; rodNum < lightningRodsSummoned.Count; rodNum++)
        {
            if(rodNum == 0)
            {
                SpawnLightning(transform.position, lightningRodsSummoned[rodNum].transform.position);
            }
            else
            {
                SpawnLightning(lightningRodsSummoned[rodNum - 1].transform.position, lightningRodsSummoned[rodNum].transform.position);
            }
            if((rodNum != 0 && rodNum == lightningRodsSummoned.Count - 1))
            {
                SpawnLightning(lightningRodsSummoned[rodNum].transform.position, transform.position);
            }
        }
        StartCoroutine(ResetLightning());
    }

    private void SpawnLightning(Vector3 rod1, Vector3 rod2)
    {
        Vector3 spawnPos = (rod1 + rod2)/2;
        spawnPos.y = 1.5f;
        GameObject newLightning = Instantiate(lightningAttackPrefab, spawnPos, Quaternion.identity);
        float z = Vector3.Distance(rod1, rod2);
        Transform sfxModel = newLightning.GetComponentsInChildren<Transform>()[1];
        newLightning.transform.LookAt(rod2);
        sfxModel.transform.localScale = new Vector3(1, z * .5f, 1);
        sfxModel.transform.Rotate(Vector3.right, 90);
        lightningSummoned.Add(newLightning);
        
        lightningSfx.Strike(rod1, rod2);
    }

    private IEnumerator ResetLightning()
    {
        yield return new WaitForSeconds(lightningLifeTime);
        foreach(GameObject lightning in lightningSummoned)
        {
            Destroy(lightning);
        }
        myAnimator.SetBool("goAttack", false);
        lightningSummoned.Clear();
    }

    private IEnumerator AttackCooldown()
    {
        lightningCooldownLeft = PlayerStats.Current.AttackCooldown;
        while(lightningCooldownLeft >= 0f)
        {
            lightningCooldownLeft -= Time.deltaTime;
            yield return null;
        }
        canAttack = true;
    }
}
