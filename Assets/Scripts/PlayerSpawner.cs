using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefabs")] public GameObject theseusPrefab;
    public GameObject minotaurPrefab;

    private Theseus theseus;
    private Minotaur minotaur;

    public Theseus GetTheseus() => theseus;
    public Minotaur GetMinotaur() => minotaur;

    public Theseus SpawnTheseus(GridManager gridManager, Vector2Int startPos)
    {
        if (theseus != null)
        {
            Destroy(theseus.gameObject);
        }

        GameObject theseusObject = Instantiate(theseusPrefab);
        theseus = theseusObject.GetComponent<Theseus>();

        if (theseus == null)
        {
            theseus = theseusObject.AddComponent<Theseus>();
        }

        theseus.Initialize(gridManager, startPos);
        return theseus;
    }

    public Minotaur SpawnMinotaur(GridManager gridManager, Vector2Int startPos)
    {
        if (minotaur != null)
        {
            Destroy(minotaur.gameObject);
        }

        GameObject minotaurObject = Instantiate(minotaurPrefab);
        minotaur = minotaurObject.GetComponent<Minotaur>();

        if (minotaur == null)
        {
            minotaur = minotaurObject.AddComponent<Minotaur>();
        }

        minotaur.Initialize(gridManager, startPos);
        return minotaur;
    }
}