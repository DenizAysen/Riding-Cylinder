using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current;
    public float limitX;
    public float xSpeed;
    public float runningSpeed;
    private float _currentRunningSpeed;

    public GameObject ridingCylinderPrefab;
    public List<RidingCylinder> cylinders;

    private bool _spawningBridge;
    public GameObject bridgePiecePrefab;    
    private BridgeSpawner _bridgeSpawner;

    private float _dropSoundTimer;

    private float _creatingBridgeTimer;

    private bool _finished;

    private float _scoretimer = 0f;

    public Animator animator;

    private float _lastTouchedX;

    public AudioSource cylinderAudioSource, triggerAudioSource, itemAudioSource;
    public AudioClip gatherAudioClip, dropAudioClip, coinAudioClip, buyAudioClip, equipItemAudioClip, unequipItemAudioClip;

    public List<GameObject> wearSpots;

    // Update is called once per frame
    void Update()
    {
        if(LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }

        float newX = 0f;
        float touchXDelta = 0f;

        if (Input.touchCount > 0 )
        {
            if(Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            else if(Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                touchXDelta =5 * (Input.GetTouch(0).position.x -_lastTouchedX ) / Screen.width;
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
           
        }
        else if (Input.GetMouseButton(0))
        {
            touchXDelta = Input.GetAxis("Mouse X");
        }

        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX, -limitX,limitX);

        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime);
        transform.position = newPosition;

        if (_spawningBridge)
        {
            PlayDropSound();
            _creatingBridgeTimer -= Time.deltaTime;
            if(_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f;
                IncrementCylinderVolume(-0.01f);
                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab,this.transform);
                createdBridgePiece.transform.SetParent(null);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position;
                float distance = direction.magnitude;
                direction = direction.normalized;
                createdBridgePiece.transform.forward = direction;
                float _characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                _characterDistance = Mathf.Clamp(_characterDistance, 0, distance);

                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * _characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePiece.transform.position = newPiecePosition;


                if (_finished)
                {
                    _scoretimer -= Time.deltaTime;
                    if(_scoretimer < 0)
                    {
                        _scoretimer = 0.3f;
                        LevelController.Current.ChangeScore(1);
                    }
                }
            }

        }

    }

    public void ChangeSpeed(float value)
    {
        _currentRunningSpeed = value;
    }

    private void OnTriggerEnter(Collider other)
    {
       
        switch (other.tag)
        {
            case "AddCylinder":
                cylinderAudioSource.PlayOneShot(gatherAudioClip, 0.1f);
                IncrementCylinderVolume(0.1f);
                Destroy(other.gameObject);
                break;
            case "SpawnBridge":
                StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
                break;
            case "StopSpawnBridge":
                StopSpawningBridge();
                if (_finished)
                {
                    LevelController.Current.FinishGame();
                }
                break;
            case "Finish":
                _finished = true;
                StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
                break;
            case "Coin":
                triggerAudioSource.PlayOneShot(coinAudioClip,0.1f);
                other.tag = "Untagged";
                LevelController.Current.ChangeScore(10);
                Destroy(other.gameObject);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (LevelController.Current.gameActive)
        {
            if (other.tag == "Trap")
            {
                PlayDropSound();
                IncrementCylinderVolume(-Time.fixedDeltaTime);
            }
        }
        
    }

    public void IncrementCylinderVolume(float value)
    {
        if(cylinders.Count == 0)
        {
            if(value > 0)
            {
                CreateCylinder(value);
            }
            else
            {
                if (_finished)
                {
                    LevelController.Current.FinishGame();
                }
                else
                {
                    Die();
                }
            }
        }
        else
        {
            cylinders[cylinders.Count - 1].IncrementCylinderValue(value);
        }
    }

    public void Die()
    {
        animator.SetBool("dead", true);
        gameObject.layer = 8;
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();
    }

    public void CreateCylinder(float value)
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderValue(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
    }

    public void StartSpawningBridge(BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner;
        _spawningBridge = true;
    }

    public void StopSpawningBridge()
    {
        _spawningBridge = false;
    }

    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if (_dropSoundTimer < 0f)
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSource.PlayOneShot(dropAudioClip, 0.1f);
        }
    }
}
