using UnityEngine;
using UnityEngine.Audio;

// Credits: Zain Al Rubaie

[RequireComponent(typeof(AudioSource))]
public class SurfaceCaster : MonoBehaviour
{
    [SerializeField] private SurfaceMap _surfaceMap; //used to get the correct audiopatch relative to each surface
    //[SerializeField] private PlayerAudio _playerAudioScript;
    private int _layerMask; //the layer that you assign to your object that you want to cast on
    private void Awake()
    {
        _layerMask = LayerMask.GetMask(new string[] { "Ground" }); //checks the string and assigns the layermask
        Debug.Assert(true, "PlayerAudio script not found!");
    }
    public void CastForSurface()
    {
        Vector3 _vector = new Vector3(0, 1, 0);
        Ray ray = new Ray((transform.position + _vector), Vector3.down);
        //makes a new ray that goes from the starting position to the end position
        bool didHit = Physics.Raycast(ray, out RaycastHit rayCastHit, 3f, _layerMask);
        if (didHit) //this boolean determines if it hit, and also contains a bunch of info about the hit
        {
            SoundSurfaceIDIdentifier surfaceId = rayCastHit.collider.GetComponent<SoundSurfaceIDIdentifier>(); //copies over the soundsurfaceid of the hit object
            AudioPatch surfacePatch = _surfaceMap.GetPatch(surfaceId.Surface); //gets the corresponding patch
            //_playerAudioScript.PlayFootstep(surfacePatch); //calls the corresponding playerAudio function to play the footsteps
        }
    }
}
