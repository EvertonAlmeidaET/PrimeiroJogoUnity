using TMPro;
using UnityEngine;

public class AlturaPersonagemFase : MonoBehaviour
{
    // variavel para pegar a referencia do pesonagem que esta no unity
    public Transform personagem;

    // Texto do canvas para o campo pontuacao
    private TMP_Text pontuacaoPersonagem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        //Inicializando a variavel texto
        pontuacaoPersonagem = GetComponent<TMP_Text>();
        pontuacaoPersonagem.text = "0";

    }

    // Update is called once per frame
    void Update()
    {
        // Pegando a altera do personagem da tela, arrendando o valor e devolvendo um texto string para o sistema de pontuacao
        //Debug.Log(personagem.position.y);
        pontuacaoPersonagem.text = Mathf.RoundToInt(personagem.position.y).ToString();
        
    }
}
