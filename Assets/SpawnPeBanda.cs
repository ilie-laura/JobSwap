using UnityEngine;
using UnityEngine.InputSystem; // Noul Input System
using System.Collections.Generic;

public class SpawnPeBanda : MonoBehaviour
{
    [Header("Referințe Bandă & Oprire")]
    public Transform banda;
    public Transform stopbanda; // Cubul unde se opresc produsele

    [Header("Clienți (Customer)")]
    public Transform punctPornireClient; // Unde apare clientul (ex: la începutul benzii)
    public Transform punctOprireClient;  // Unde se oprește clientul la casă
    public GameObject[] prefabsClienti;  // Cele 2 tipuri de cuburi/modele de client

    [Header("Produse (Assets)")]
    public GameObject[] prefabsProduse;  // Cele 5 produse
    public int minProduse = 1;
    public int maxProduse = 4;
    public float spatiereProduse = 0.4f;

    [Header("Ajustare Produse")]
    public Vector3 scaraProdus = new Vector3(1f, 1f, 1f);
    public float inaltimeDeasupraBanda = 0.35f;

    [Header("Setări Mișcare")]
    public float vitezaBanda = 0.8f;
    public float vitezaClient = 1.5f;
    public float distantaOprireProduse = 0.3f;

    // Stări interne
    private List<GameObject> produsePeBanda = new List<GameObject>();
    private GameObject clientCurent;
    private bool clientLaCasa = false;
    private bool produseOprite = false;

    void Start()
    {
        GenereazaClientNou();
    }

    void Update()
    {
        HandeMiscaClient();
        HandleMiscaProduse();
        HandleScanareProduse();
    }

    // 1. Generare Client Nou și Produse
    public void GenereazaClientNou()
    {
        if (prefabsClienti.Length == 0 || prefabsProduse.Length == 0) return;

        // Curățăm produsele vechi
        CurataBanda();

        // Spawnează un client aleatoriu din cele 2 tipuri
        int indexClient = Random.Range(0, prefabsClienti.Length);
        clientCurent = Instantiate(prefabsClienti[indexClient], punctPornireClient.position, punctPornireClient.rotation);
        clientLaCasa = false;
        produseOprite = false;

        // Alege un număr aleatoriu de produse pentru acest client
        int numarProduse = Random.Range(minProduse, maxProduse + 1);

        float startOffset = -((numarProduse - 1) * spatiereProduse) / 2f;

        for (int i = 0; i < numarProduse; i++)
        {
            int indexProdus = Random.Range(0, prefabsProduse.Length);
            GameObject prefabAles = prefabsProduse[indexProdus];

            Vector3 pozitieGlobala = banda.position
                                   + (banda.up * inaltimeDeasupraBanda)
                                   + (banda.right * (startOffset + (i * spatiereProduse)));

            GameObject produsCreat = Instantiate(prefabAles, pozitieGlobala, Quaternion.identity);

            produsCreat.transform.localScale = scaraProdus;
            produsCreat.transform.rotation = banda.rotation;
            produsCreat.transform.SetParent(banda, true);

            produsePeBanda.Add(produsCreat);
        }
    }

    // 2. Mișcare Client până la Punctul de Oprire
    void HandeMiscaClient()
    {
        if (clientCurent == null || punctOprireClient == null) return;

        if (Vector3.Distance(clientCurent.transform.position, punctOprireClient.position) > 0.1f)
        {
            clientCurent.transform.position = Vector3.MoveTowards(
                clientCurent.transform.position,
                punctOprireClient.position,
                vitezaClient * Time.deltaTime
            );
        }
        else
        {
            clientLaCasa = true;
        }
    }

    // 3. Mișcare Produse pe Bandă
    void HandleMiscaProduse()
    {
        if (produsePeBanda.Count == 0 || produseOprite) return;

        // Verificăm dacă primul produs a atins stopbanda
        if (stopbanda != null && produsePeBanda[0] != null)
        {
            float distanta = Vector3.Distance(produsePeBanda[0].transform.position, stopbanda.position);
            if (distanta <= distantaOprireProduse)
            {
                produseOprite = true;
                return;
            }
        }

        // Deplasare produse spre stânga
        foreach (GameObject prod in produsePeBanda)
        {
            if (prod != null)
            {
                prod.transform.Translate(-Vector3.right * vitezaBanda * Time.deltaTime, Space.Self);
            }
        }
    }

    // 4. Scanare Produse pe tasta E
    void HandleScanareProduse()
    {
        // Preluare apăsare tasta E în Input System
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Scanăm doar dacă produsele au ajuns la oprire
            if (produseOprite && produsePeBanda.Count > 0)
            {
                // Eliminăm primul produs din listă (îl "scanăm")
                GameObject produsScanat = produsePeBanda[0];
                produsePeBanda.RemoveAt(0);
                Destroy(produsScanat);

                Debug.Log("Produs scanat!");

                // Dacă am scanat toate produsele clientului
                if (produsePeBanda.Count == 0)
                {
                    Debug.Log("Client finalizat! Vine următorul...");
                    Destroy(clientCurent);

                    // Așteaptă puțin și generează următorul client
                    Invoke("GenereazaClientNou", 1.0f);
                }
                else
                {
                    // Pornim banda din nou pentru a aduce următorul produs în față
                    produseOprite = false;
                }
            }
        }
    }

    void CurataBanda()
    {
        foreach (GameObject prod in produsePeBanda)
        {
            if (prod != null) Destroy(prod);
        }
        produsePeBanda.Clear();
        if (clientCurent != null) Destroy(clientCurent);
    }
}