using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardGunScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage;
    [Tooltip("Range: 100 -> dynamicDamagaeRange")]
    [SerializeField] private float dynamicDamageRange;
    [SerializeField] private float maxShootingRange;
    [Tooltip("Spread radius in meters after a distance of 5 meters")]
    [SerializeField] private float spreadRadius;
    [SerializeField] private float roundsPerSecond;
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



            Vector3 DesiredDestination = camera.Reticle.transform.position;           
            Ray ray = camera.GetCamera().ScreenPointToRay(camera.Reticle.transform.position);
            ray.direction += camera.GetCamera().transform.TransformDirection(Random.insideUnitCircle * spreadRadius / 10);
            
            RaycastHit info;            
            if (Physics.Raycast(ray, out info, maxShootingRange))
            {              
                if(impact != null)
                    GameObject.Instantiate(impact, info.point, Quaternion.LookRotation(transform.forward * -1));

                if(info.transform.GetComponent<Targetable>() != null)
                {
                    //Add dynamic damagage here
;                    info.transform.GetComponent<Targetable>().ReceiveDamage(this, new DamageEventArgs(damage));
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
