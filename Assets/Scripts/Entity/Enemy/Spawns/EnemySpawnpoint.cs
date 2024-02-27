using UnityEngine;
using Photon.Pun;

public class EnemySpawnpoint : MonoBehaviour {

    public string prefab;
    public GameObject currentEntity;

    public bool isSpawning = false;

    public virtual bool AttemptSpawning() {
      
        if (currentEntity)
            return false;

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, 1.5f)) {
            if (hit.gameObject.CompareTag("Player"))
                //cant spawn here
                return false;
        }

        currentEntity = PhotonNetwork.InstantiateRoomObject(prefab, transform.position, transform.rotation);
        
        return true;
    }

    public virtual bool AttemptSpawning1v1() {//ACCURACY: DISTANCE BASED RESPAWNING
        if (currentEntity)
            return false;

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, 7.5f)) {
            if (hit.gameObject.CompareTag("Player"))
                //cant spawn here
                return false;
        }
        if(prefab == "" && !isSpawning && GameManager.Instance.getPiranaplantCanspawn()){//WORKAROUND FOR PIRANA PLANTS PART 1
            StartCoroutine(piranaplantWorkaround());
            return true;
        }else if(prefab == "" && isSpawning){
            return false;
        }else if(prefab == ""){
            return false;
        }
        
        currentEntity = PhotonNetwork.InstantiateRoomObject(prefab, transform.position, transform.rotation);
        return true;
    }

    private System.Collections.IEnumerator piranaplantWorkaround() //WORKAROUND FOR PIRANA PLANTS PART 2
    {

        if(!isSpawning){
            isSpawning = true;
            yield return new WaitForSeconds(3f);
            isSpawning = false;
            AttemptSpawning();
        }
        yield return false;
        
    }

    public void OnDrawGizmos() {
        string icon = prefab.Split("/")[^1];
        float offset = prefab switch {
            "Prefabs/Enemy/BlueKoopa" => 0.15f,
            "Prefabs/Enemy/RedKoopa" => 0.15f,
            "Prefabs/Enemy/Koopa" => 0.15f,
            "Prefabs/Enemy/Bobomb" => 0.22f,
            "Prefabs/Enemy/Goomba" => 0.22f,
            "Prefabs/Enemy/Spiny" => -0.03125f,
            _ => 0,
        };
        Gizmos.DrawIcon(transform.position + offset * Vector3.up, icon, true, new Color(1, 1, 1, 0.5f));
    }
}
