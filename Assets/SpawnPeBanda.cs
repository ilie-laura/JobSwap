using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Necesar pentru TextMeshPro
using System.Collections.Generic;

public class SpawnPeBanda : MonoBehaviour
{
    [Header("Referințe Bandă & Oprire")]
    public Transform banda;
    public Transform stopbanda;

    [Header("Clienți (Customer)")]
    public Transform punctPornireClient;
    public Transform punctOprireClient;
    public GameObject[] prefabsClienti;

    [Header("Produse (Assets)")]
    public GameObject[] prefabsProduse;
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

    [Header("Ecran Casă & Scor")]
    public TextMeshPro ecranText; // Drag & Drop textul de pe casa de marcat
    private float scorTotal = 0f;

    // Stări interne
    private List<GameObject> produsePeBanda = new List<GameObject>();
    private GameObject clientCurent;
    private bool clientLaCasa = false;
    private bool produseOprite = false;

    void Start()
    {
        ActualizeazaEcran("Open", 0f);
        GenereazaClientNou();
    }

    void Update()
    {
        HandeMiscaClient();
        HandleMiscaProduse();
        HandleScanareProduse();
    }

    public void GenereazaClientNou()
    {
        if (prefabsClienti.Length == 0 || prefabsProduse.Length == 0) return;

        CurataBanda();

        int indexClient = Random.Range(0, prefabsClienti.Length);
        clientCurent = Instantiate(prefabsClienti[indexClient], punctPornireClient.position, punctPornireClient.rotation);
        clientLaCasa = false;
        produseOprite = false;

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

    void HandleMiscaProduse()
    {
        if (produsePeBanda.Count == 0 || produseOprite) return;

        if (stopbanda != null && produsePeBanda[0] != null)
        {
            float distanta = Vector3.Distance(produsePeBanda[0].transform.position, stopbanda.position);
            if (distanta <= distantaOprireProduse)
            {
                produseOprite = true;
                return;
            }
        }

        foreach (GameObject prod in produsePeBanda)
        {
            if (prod != null)
            {
                prod.transform.Translate(-Vector3.right * vitezaBanda * Time.deltaTime, Space.Self);
            }
        }
    }

    void HandleScanareProduse()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (produseOprite && produsePeBanda.Count > 0)
            {
                GameObject produsScanat = produsePeBanda[0];

                // Preluăm componenta Produs de pe obiectul scanat
                Produs infoProdus = produsScanat.GetComponent<Produs>();

                string nume = "Produs";
                float pret = 1.0f;

                if (infoProdus != null)
                {
                    nume = infoProdus.numeProdus;
                    pret = infoProdus.pret;
                }

                // Adăugăm la scor
                scorTotal += pret;

                // Actualizăm afișajul de pe ecran
                ActualizeazaEcran(nume, pret);

                // Eliminăm produsul
                produsePeBanda.RemoveAt(0);
                Destroy(produsScanat);

                if (produsePeBanda.Count == 0)
                {
                    Destroy(clientCurent);
                    Invoke("GenereazaClientNou", 1.5f);
                }
                else
                {
                    produseOprite = false;
                }
            }
        }
    }

    void ActualizeazaEcran(string numeProdus, float pret)
    {
        if (ecranText != null)
        {
            ecranText.text = $"{numeProdus}\nPret: {pret:F2} LEI\n----------\nTotal: {scorTotal:F2} LEI";
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