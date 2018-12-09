using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardGunScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float damage;
    [Tooltip("Range: 100 -> dynamicDamagaeRange")]
    [SerializeField] private float dynamicDamageRange;
    [SerializeField] private float maxShootingRange;
    [SerializeField] private float roundsPerSecond;
    [SerializeField] private float spreadRadius;
    [SerializeField] private int magazineSize;
    [SerializeField] private float reloadTime; 

    [Header("Visual")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impact;

    [Header("Sounds")]
    [SerializeField] private AudioSource ReloadSound;
    [SerializeField] private AudioSource ShootingSound;

    private float currentShootingInterval = 0f;
    private float CurrentMagazineSize;
    private Coroutine Reloading;
    private Animator AnimationController;

    private void Awake()
    {
       // AnimationController = GetComponent<Animator>();
        CurrentMagazineSize = magazineSize;
    }

    private void Update()
    {
        currentShootingInterval += Time.deltaTime;             
    }


    public void Fire(Player.PlayerCamera camera)
    {
        //AnimationController.SetBool("isReloading", Reloading == null);

        if (Reloading != null) return;

        if (currentShootingInterval >= 1 / roundsPerSecond)
        {
            //AnimationController.SetTrigger("Shoot");

            currentShootingInterval = 0f;
            CurrentMagazineSize -= 1;

            if (ShootingSound != null)
                ShootingSound.Play();
            
            if(muzzleFlash != null)
                muzzleFlash.Play();


            float theta = Random.Range(0f, 1f) * 2 * Mathf.PI;
            float r = spreadRadius * Mathf.Sqrt(Random.Range(0f, 1f));

            Vector3 DesiredDestination = camera.Reticle.transform.position;

            DesiredDestination.x = DesiredDestination.x + r * Mathf.Cos(theta);
            DesiredDestination.y = DesiredDestination.y + r * Mathf.Sin(theta);

            Ray ray = camera.GetCamera().ScreenPointToRay(DesiredDestination);

            RaycastHit info;
            if(Physics.Raycast(ray, out info, maxShootingRange))
            {              
                if(impact != null)
                    GameObject.Instantiate(impact, info.point, Quaternion.LookRotation(transform.forward * -1));

                if(info.transform.GetComponent<Targetable>() != null)
                {
                    info.transform.GetComponent<Targetable>().ReceiveDamagea(this, new DamageEventArgs(Random.Range(damage, damage * dynamicDamageRange / 100)));
                }
            }

        }

        if (CurrentMagazineSize <= 0)
            Reload();
        
    }


    IEnumerator ReloadEvent()
    {
        float elapsedReloadTime = 0;
        //Play reloadanimtion + reload sound
        if (ReloadSound != null)
            ReloadSound.Play();

        while (elapsedReloadTime < reloadTime)
        {
            elapsedReloadTime += Time.deltaTime;
            yield return null;                 
        }

        CurrentMagazineSize = magazineSize;

        Reloading = null;


    }

    public void Reload()
    {
        if (Reloading == null)
        {
            Reloading = StartCoroutine(ReloadEvent());
            
        }
    }
        
        
}
