using UnityEngine;
using System.Collections.Generic;

public class SpawnPeBanda : MonoBehaviour
{
    [Header("Referințe Suporturi")]
    public Transform banda; // Cubul pe care glisează produsele
    public Transform stopbanda; // Cubul de oprire (Senzor / Capăt bandă)

    [Header("Assets (Modele 3D)")]
    public GameObject[] prefabsAssets;

    [Header("Setări Generare")]
    public int numarObiecte = 3;
    public float spatiere = 0.5f;

    [Header("Ajustare Scală & Poziție")]
    public Vector3 scaraObiect = new Vector3(1f, 1f, 1f);
    public float inaltimeDeasupra = 0.35f;
    public Vector3 rotatieAjustata = Vector3.zero;

    [Header("Mișcare Bandă")]
    public bool seMisca = true;
    public float vitezaBanda = 0.8f;
    public float distantaOprire = 0.3f; // Cât de aproape de 'stopbanda' să se oprească primul obiect

    private List<GameObject> obiecteGenerate = new List<GameObject>();

    void Start()
    {
        GenereazaObiecte();
    }

    void Update()
    {
        if (seMisca && obiecteGenerate.Count > 0)
        {
            MiscaObiecteleSpreStanga();
        }
    }

    [ContextMenu("Genereaza Acum")]
    public void GenereazaObiecte()
    {
        if (prefabsAssets.Length == 0 || banda == null) return;

        obiecteGenerate.Clear();
        for (int i = banda.childCount - 1; i >= 0; i--)
        {
            Transform child = banda.GetChild(i);
            if (child.gameObject.name.Contains("(Clone)"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        float startOffset = -((numarObiecte - 1) * spatiere) / 2f;

        for (int i = 0; i < numarObiecte; i++)
        {
            int indexAleatoriu = Random.Range(0, prefabsAssets.Length);
            GameObject prefabAles = prefabsAssets[indexAleatoriu];

            Vector3 pozitieGlobala = banda.position
                                   + (banda.up * inaltimeDeasupra)
                                   + (banda.right * (startOffset + (i * spatiere)));

            GameObject obiectCreat = Instantiate(prefabAles, pozitieGlobala, Quaternion.identity);

            obiectCreat.transform.localScale = scaraObiect;
            obiectCreat.transform.rotation = banda.rotation * Quaternion.Euler(rotatieAjustata);
            obiectCreat.transform.SetParent(banda, true);

            obiecteGenerate.Add(obiectCreat);
        }
    }

    void MiscaObiecteleSpreStanga()
    {
        // 1. Verificăm dacă vreun obiect a ajuns deja la obiectul stopbanda
        bool trebuieOprit = false;

        if (stopbanda != null)
        {
            foreach (GameObject obj in obiecteGenerate)
            {
                if (obj != null)
                {
                    // Calculăm distanța dintre produs și cubul de oprire
                    float distanta = Vector3.Distance(obj.transform.position, stopbanda.position);
                    if (distanta <= distantaOprire)
                    {
                        trebuieOprit = true;
                        break;
                    }
                }
            }
        }

        // 2. Dacă primul obiect a atins stopbanda, oprim mișcarea întregului șir
        if (trebuieOprit) return;

        // 3. Deplasare spre STÂNGA (-Vector3.right)
        foreach (GameObject obj in obiecteGenerate)
        {
            if (obj != null)
            {
                // Schimbat în -Vector3.right pentru a merge spre stânga
                obj.transform.Translate(-Vector3.right * vitezaBanda * Time.deltaTime, Space.Self);
            }
        }
    }
}