using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefabs")] public GameObject theseusPrefab;
    public GameObject minotaurPrefab;

    private Theseus theseus;
    private Minotaur minotaur;

    public Theseus SpawnTheseus(Vector2Int startPos)
    {
        if (theseus != null)
        {
            theseus.Initialize(startPos);
            return theseus;
        }

        GameObject theseusObject = Instantiate(theseusPrefab);
        theseus = theseusObject.GetComponent<Theseus>();

        if (theseus == null)
        {
            theseus = theseusObject.AddComponent<Theseus>();
        }

        theseus.Initialize(startPos);
        return theseus;
    }

    public Minotaur SpawnMinotaur(Vector2Int startPos)
    {
        if (minotaur != null)
        {
            minotaur.Initialize(startPos);
            return minotaur;
        }

        GameObject minotaurObject = Instantiate(minotaurPrefab);
        minotaur = minotaurObject.GetComponent<Minotaur>();

        if (minotaur == null)
        {
            minotaur = minotaurObject.AddComponent<Minotaur>();
        }

        minotaur.Initialize(startPos);
        return minotaur;
    }
}