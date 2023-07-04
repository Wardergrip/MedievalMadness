using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class PickUp : MonoBehaviour
{
    // STATICS
    public static GameObject SpawnEmptyPickUp(Vector3 pos, Quaternion rot)
    {
        return Instantiate(Resources.Load("EmptyPickUp") as GameObject,pos,rot);
    }

    public static GameObject SpawnPickUp(Vector3 pos, Quaternion rot, GameObject weaponPrefab)
    {
        GameObject pickUp = SpawnEmptyPickUp(pos, rot);
        pickUp.GetComponent<PickUp>().SetAndSpawnWeaponPrefab(weaponPrefab);
        pickUp.name = $"[PickUp] {weaponPrefab.name}";
        return pickUp;
    }

    // NON-STATICS

    [SerializeField] private GameObject _buttonPromptCanvas;
    private PlayerPawn _lastPlayer;
    private Collider _lastPlayerCollider;

    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private GameObject _weaponPrefab;
    private GameObject _weapon;
    [SerializeField] private Transform _weaponTransform;

    public UnityEvent PickUpEvent;

    private string _ignoreRaycastLayer = "Ignore Raycast";
    private void SetAndSpawnWeaponPrefab(GameObject prefab)
    {
        // If weapon already created, return
        if (_weapon != null) return;

        // Set weapon prefab and spawn
        _weaponPrefab = prefab;
        _weapon = Instantiate(prefab, _weaponTransform);

        // Set layer to spawned weapon
        _weapon.layer = LayerMask.NameToLayer(_ignoreRaycastLayer);
        foreach (Transform child in _weapon.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(_ignoreRaycastLayer);
        }

        // Deactivate weapon
        _weapon.GetComponent<Weapon>().WeaponActivate(false);

        // Check if weapon has pickUpAble
        var pickUpAble = _weapon.GetComponent<PickUpAble>();
        if (pickUpAble == null)
        {
            pickUpAble = _weapon.GetComponentInChildren<PickUpAble>();
        }
        Debug.Assert(pickUpAble, $"{prefab.name} does not have a pickUpAble.");

        // Make sure we have a boxCollider to conform
        if (_boxCollider == null)
        {
            _boxCollider = GetComponent<BoxCollider>();
            Debug.Assert(_boxCollider, $"No boxCollider found in {nameof(SetAndSpawnWeaponPrefab)}");
        }
        _boxCollider.center = pickUpAble.GetBox.center;
        _boxCollider.size = pickUpAble.GetBox.size;

        // Enable collider
        pickUpAble.GetBox.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
        Debug.Assert(_boxCollider.isTrigger, "Boxcollider must be trigger");
        _buttonPromptCanvas.SetActive(false);
        if (_weaponPrefab)
        {
            SetAndSpawnWeaponPrefab(_weaponPrefab);
        }
        else StartCoroutine(WeaponPrefabSafetyGuardCoroutine());
    }

    private IEnumerator WeaponPrefabSafetyGuardCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Assert(_weaponPrefab, $"No weaponPrefab assigned! Either you forgot to assign a prefab in the inspector or a script forgot to assign it during runtime.");
    }

    //private void OnTriggerEnter(Collider other)
    //{
        //if (other.GetComponent<Weapon>()) return;

        //PlayerPawn player = other.gameObject.GetComponentInParent<PlayerPawn>();
        //if (player)
        //{
        //    Debug.Log("Player Enter");
        //    _lastPlayer = player;
        //    _buttonPromptCanvas.SetActive(true);
        //    player.LastPickUp = this;
        //}
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Weapon>()) return;

        PlayerPawn player = other.gameObject.GetComponentInParent<PlayerPawn>();
        if (player)
        {
            _lastPlayer = player;
            _buttonPromptCanvas.SetActive(true);
            player.LastPickUp = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Weapon>()) return;

        PlayerPawn player = other.gameObject.GetComponentInParent<PlayerPawn>();
        if (player)
        {
            if (player.LastPickUp == this)
            {
                _buttonPromptCanvas.SetActive(false);
                player.LastPickUp = null;
            }
            else if (player == _lastPlayer)
            {
                _buttonPromptCanvas.SetActive(false);
            }
        }
    }

    public GameObject PickUpWeaponPrefab()
    {
        PickUpEvent?.Invoke();
        Destroy(gameObject);
        return _weaponPrefab;
    }
}
