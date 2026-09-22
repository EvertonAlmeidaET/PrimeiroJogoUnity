using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ============================================================
    // MOVIMENTO
    // ============================================================

    // Velocidade com que o jogador anda.
    // Como é public, podemos alterar esse valor diretamente
    // pelo Inspector do Unity.
    public float speed = 5f;


    // ============================================================
    // PULO
    // ============================================================

    // Altura desejada para o pulo.
    public float jumpHeight = 1.5f;

    // Gravidade aplicada ao personagem.
    //
    // O CharacterController não possui física como um Rigidbody.
    // Por isso precisamos aplicar a gravidade manualmente.
    //
    // O valor é negativo porque queremos que o personagem
    // seja puxado para baixo.
    public float gravity = -20f;


    // ============================================================
    // CÂMERA
    // ============================================================

    // Sensibilidade do mouse.
    // Quanto maior o valor, mais rápido a câmera gira.
    public float mouseSensitivity = 2f;

    // Referência para a câmera que está dentro do Player.
    //
    // No Inspector devemos arrastar a Main Camera para esse campo.
    public Transform cameraTransform;


    // ============================================================
    // VARIÁVEIS INTERNAS
    // ============================================================

    // Referência para o CharacterController que está no Player.
    //
    // Usaremos essa variável para mandar o personagem se mover
    // através de controller.Move().
    private CharacterController controller;


    // Guarda a velocidade atual do personagem.
    //
    // velocity.y representa a velocidade vertical:
    //
    // valor positivo → subindo
    // 0              → ponto mais alto
    // valor negativo → caindo
    //
    // A gravidade altera esse valor a cada frame.
    private Vector3 velocity;


    // Guarda a rotação vertical da câmera.
    //
    // Usamos uma variável separada porque queremos que:
    //
    // Player → gire para esquerda/direita
    //
    // Camera → olhe para cima/baixo
    //
    // Dessa forma o corpo do personagem não fica inclinando.
    private float cameraRotationX;


    // Indica que o jogador pediu para pular.
    //
    // Quando detectamos que o Space acabou de ser pressionado,
    // colocamos:
    //
    // querPular = true
    //
    // Depois que o pulo é executado:
    //
    // querPular = false
    private bool querPular;


    // Guarda se o Space estava pressionado no frame anterior.
    //
    // Vamos usar isso para detectar a transição:
    //
    // NÃO pressionado → pressionado
    //
    // Isso nos permite criar nosso próprio "pressionou agora",
    // em vez de depender diretamente do wasPressedThisFrame.
    private bool spaceEstavaPressionado;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        // Procura no próprio Player um componente
        // CharacterController.
        //
        // Assim não precisamos arrastar o CharacterController
        // manualmente para uma variável.
        controller = GetComponent<CharacterController>();


        // Trava o cursor no centro da janela do jogo.
        //
        // Isso é comum em jogos FPS porque o mouse precisa
        // controlar a câmera continuamente.
        Cursor.lockState = CursorLockMode.Locked;


        // Esconde o cursor enquanto estamos jogando.
        Cursor.visible = false;
    }


    // ============================================================
    // UPDATE
    // ============================================================

    // Update é executado uma vez por frame.
    //
    // É adequado para:
    //
    // - Ler teclado
    // - Ler mouse
    // - Controlar câmera
    // - Controlar entrada do jogador
    private void Update()
    {
        // Primeiro verificamos o que o jogador está fazendo
        // no teclado.
        LerEntrada();


        // Depois calculamos o movimento do personagem.
        Mover();


        // Por último calculamos o movimento da câmera.
        Olhar();
    }


    // ============================================================
    // LEITURA DO TECLADO
    // ============================================================

    private void LerEntrada()
    {
        // Obtém o teclado através do novo Input System.
        Keyboard teclado = Keyboard.current;


        // Segurança:
        //
        // Se não existir um teclado disponível, não fazemos nada.
        if (teclado == null)
            return;


        // ========================================================
        // DETECÇÃO DO SPACE
        // ========================================================

        // isPressed retorna true enquanto a tecla estiver sendo
        // pressionada.
        //
        // Exemplo:
        //
        // segurou Space por 1 segundo
        //
        // isPressed ficará true durante aproximadamente
        // todos os frames desse segundo.
        bool spacePressionado = teclado.spaceKey.isPressed;


        // Aqui detectamos se o Space acabou de ser pressionado.
        //
        // Precisamos das duas condições:
        //
        // spacePressionado == true
        //
        // significa que o Space está pressionado AGORA.
        //
        // spaceEstavaPressionado == false
        //
        // significa que no frame anterior ele NÃO estava pressionado.
        //
        // Portanto:
        //
        // false → true
        //
        // significa que o jogador acabou de apertar a tecla.
        if (spacePressionado && !spaceEstavaPressionado)
        {
            // Registramos a intenção de pular.
            //
            // O personagem ainda não pula aqui.
            // Estamos apenas dizendo:
            //
            // "O jogador pediu um pulo."
            querPular = true;
        }


        // Agora guardamos o estado atual do Space.
        //
        // No próximo frame vamos comparar esse valor com
        // o novo estado da tecla.
        spaceEstavaPressionado = spacePressionado;
    }


    // ============================================================
    // MOVIMENTO DO PLAYER
    // ============================================================

    private void Mover()
    {
        // ========================================================
        // LEITURA DO WASD
        // ========================================================

        // Vector2 possui dois eixos:
        //
        // X → esquerda / direita
        // Y → frente / trás
        //
        // Começamos sem movimento.
        Vector2 entrada = Vector2.zero;


        // Pegamos o teclado.
        Keyboard teclado = Keyboard.current;


        // W = andar para frente.
        if (teclado.wKey.isPressed)
            entrada.y += 1;


        // S = andar para trás.
        if (teclado.sKey.isPressed)
            entrada.y -= 1;


        // A = andar para esquerda.
        if (teclado.aKey.isPressed)
            entrada.x -= 1;


        // D = andar para direita.
        if (teclado.dKey.isPressed)
            entrada.x += 1;


        // ========================================================
        // EVITAR VELOCIDADE MAIOR NA DIAGONAL
        // ========================================================

        // Se pressionarmos W + D ao mesmo tempo, teríamos:
        //
        // X = 1
        // Y = 1
        //
        // O tamanho desse vetor é maior que 1.
        //
        // ClampMagnitude limita o tamanho máximo do vetor
        // para 1.
        //
        // Assim andar na diagonal não fica mais rápido
        // que andar em linha reta.
        entrada = Vector2.ClampMagnitude(entrada, 1f);


        // ========================================================
        // TRANSFORMAR O INPUT EM MOVIMENTO 3D
        // ========================================================

        // transform.right representa o lado direito do Player.
        //
        // transform.forward representa a frente do Player.
        //
        // Isso é importante porque queremos que:
        //
        // W = frente do personagem
        //
        // e não simplesmente:
        //
        // W = eixo Z global.
        Vector3 movimento =
            transform.right * entrada.x +
            transform.forward * entrada.y;


        // ========================================================
        // VERIFICAR SE ESTAMOS NO CHÃO
        // ========================================================

        // O CharacterController possui a propriedade isGrounded.
        //
        // true  → personagem está no chão
        // false → personagem está no ar
        bool noChao = controller.isGrounded;


        // ========================================================
        // PULO
        // ========================================================

        if (noChao)
        {
            // ----------------------------------------------------
            // MANTER O PERSONAGEM NO CHÃO
            // ----------------------------------------------------

            // Quando estamos no chão, a velocidade vertical
            // provavelmente está negativa por causa da gravidade.
            //
            // Colocamos um pequeno valor negativo para manter
            // o CharacterController "grudado" no chão.
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }


            // ----------------------------------------------------
            // EXECUTAR O PULO
            // ----------------------------------------------------

            // Verificamos se o jogador pediu para pular.
            if (querPular)
            {
                // Calculamos a velocidade inicial necessária
                // para atingir a altura definida em jumpHeight.
                //
                // Exemplo:
                //
                // jumpHeight = 1.5
                //
                // O resultado será uma velocidade vertical
                // positiva que fará o personagem subir.
                velocity.y = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );


                // O comando já foi utilizado.
                //
                // Isso é importante para que o mesmo pedido
                // não provoque vários pulos.
                querPular = false;
            }
        }
        else
        {
            // ====================================================
            // ESTAMOS NO AR
            // ====================================================

            // Se o jogador apertou Space enquanto estava no ar,
            // ignoramos esse pedido.
            //
            // Assim não existe double jump.
            querPular = false;
        }


        // ========================================================
        // GRAVIDADE
        // ========================================================

        // A gravidade altera a velocidade vertical a cada frame.
        //
        // Como gravity é negativo:
        //
        // velocity.y positivo → personagem sobe
        //
        // velocity.y diminui → personagem perde velocidade
        //
        // velocity.y chega a 0 → ponto mais alto
        //
        // velocity.y negativo → personagem começa a cair
        velocity.y += gravity * Time.deltaTime;


        // ========================================================
        // MOVIMENTO FINAL
        // ========================================================

        // Movimento horizontal:
        //
        // movimento * speed
        //
        // Movimento vertical:
        //
        // Vector3.up * velocity.y
        //
        // Vector3.up é:
        //
        // (0, 1, 0)
        //
        // Portanto estamos adicionando a velocidade vertical
        // somente ao eixo Y.
        Vector3 movimentoFinal =
            movimento * speed +
            Vector3.up * velocity.y;


        // Finalmente mandamos o CharacterController executar
        // o movimento.
        //
        // Time.deltaTime transforma velocidade por segundo
        // em deslocamento correspondente a este frame.
        controller.Move(
            movimentoFinal * Time.deltaTime
        );
    }


    // ============================================================
    // CÂMERA / MOUSE
    // ============================================================

    private void Olhar()
    {
        // Lê quanto o mouse se movimentou desde o último frame.
        //
        // X → esquerda / direita
        // Y → cima / baixo
        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();


        // Movimento horizontal do mouse.
        float mouseX =
            mouseDelta.x *
            mouseSensitivity *
            Time.deltaTime;


        // Movimento vertical do mouse.
        float mouseY =
            mouseDelta.y *
            mouseSensitivity *
            Time.deltaTime;


        // ========================================================
        // ROTAÇÃO HORIZONTAL
        // ========================================================

        // Giramos o Player inteiro no eixo Y.
        //
        // Como a câmera é filha do Player, ela acompanha
        // automaticamente essa rotação.
        //
        // Resultado:
        //
        // mouse para direita
        //        ↓
        // Player gira para direita
        transform.Rotate(
            Vector3.up * mouseX
        );


        // ========================================================
        // ROTAÇÃO VERTICAL
        // ========================================================

        // Aqui alteramos somente a rotação vertical da câmera.
        //
        // Não queremos inclinar o corpo inteiro do Player.
        cameraRotationX -= mouseY;


        // Impede a câmera de passar de 90 graus para cima
        // ou para baixo.
        //
        // Sem isso seria possível virar a câmera completamente
        // de cabeça para baixo.
        cameraRotationX =
            Mathf.Clamp(
                cameraRotationX,
                -90f,
                90f
            );


        // Aplica a rotação somente na câmera.
        cameraTransform.localRotation =
            Quaternion.Euler(
                cameraRotationX,
                0f,
                0f
            );
    }
}