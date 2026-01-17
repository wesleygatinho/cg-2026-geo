## Objetivo do Jogo
- App Android de Realidade Aumentada para ensino de geometria, com fases curtas, pontuação e feedback.
- O aluno aponta a câmera, detecta o plano (chão/mesa), “coloca” objetos 3D no ambiente e resolve desafios de geometria (2D/3D).

## Stack e Decisões Técnicas
- Unity LTS (recomendado 2022.3 LTS ou 2021.3 LTS).
- AR Foundation + ARCore XR Plugin (melhor integração do que usar ARCore “puro”, e continua atendendo ARCore no Android).
- Input System + UI (Canvas) + TextMeshPro.
- URP (opcional, mas ajuda a padronizar render e performance mobile).

## Estrutura do Projeto (Scenes/Prefabs)
- **Scenes**
  - Boot: checa permissões/capacidade AR e carrega o menu.
  - Menu: seleção de módulos/fases.
  - ARGameplay: cena principal com ARSession, detecção e gameplay.
  - Results: resumo, acertos, tempo, recordes.
- **Prefabs**
  - XR Origin/AR Session setup.
  - Plane Visualizer (ativar/desativar).
  - Reticle (mira no ponto de colocação).
  - Shape (base) + variantes (cubo, prisma, pirâmide, cilindro, cone, esfera, triângulo, retângulo, círculo, etc.).
  - UI HUD (objetivo, resposta, botões, timer, score).

## Núcleo de AR (funcionalidades)
- Plane detection + AR Raycast: escolher onde colocar o objeto.
- Anchors: fixar objeto no mundo.
- Manipulação no mundo:
  - Toque para selecionar.
  - Gestos: arrastar (reposicionar), pinça (escala), rotação com dois dedos.
- Robustez:
  - Estados de tracking (perdido/limitado), mensagens ao usuário.
  - Permissão de câmera e validação de suporte ARCore.

## Mecânicas de Ensino (Game Loop)
- Loop básico por fase:
  1) Exibir objetivo (ex: “Qual o volume do cubo de aresta 0,25 m?”)
  2) Colocar forma no ambiente
  3) Mostrar medidas (ou pedir que o aluno meça)
  4) Aluno responde (múltipla escolha ou numérica)
  5) Feedback + pontuação + próxima questão
- Modos de questão (para ficar “completo” e variado):
  - **Reconhecimento**: identificar a forma correta.
  - **Medidas**: área, perímetro, volume, área lateral/total.
  - **Montagem**: escolher a forma que satisfaz uma condição (ex: volume alvo).
  - **Comparação**: “qual tem maior área/volume?”

## Conteúdo de Geometria (MVP + Expansão)
- MVP (entrega forte, viável):
  - 2D: triângulo, retângulo, círculo (perímetro/área).
  - 3D: cubo, paralelepípedo, cilindro, esfera (volume/área).
  - Banco de questões simples configurável (JSON/ScriptableObjects).
- Expansão:
  - Prisma/pirâmide/cone.
  - Questões com tolerância numérica, unidades (cm/m) e arredondamento.

## Sistema de Questões e Dados
- Modelo de dados:
  - Questão: tipo, forma, parâmetros (medidas), resposta correta, alternativas, tolerância.
- Autorias:
  - ScriptableObjects para editar no Inspector (rápido para disciplina).
  - Opcional: carregar de JSON para facilitar adicionar questões.

## UI/UX
- HUD:
  - Instruções de AR (“mova o celular para detectar plano”).
  - Objetivo atual + botão “Mostrar fórmula” (se permitido).
  - Entrada: múltipla escolha e/ou teclado numérico.
  - Timer opcional.
- Acessibilidade:
  - Fontes legíveis, contraste, feedback sonoro opcional.

## Pontuação e Progressão
- Score: acerto, tempo, tentativas.
- Sequência de fases: desbloqueio por desempenho ou livre.
- Persistência local: PlayerPrefs (records) ou arquivo simples (JSON) para histórico.

## Performance e Qualidade
- Otimizações:
  - Limitar malhas e materiais, batching simples.
  - Desativar visualização de planos após posicionar.
- Compatibilidade:
  - Android ARM64, IL2CPP, permissões corretas.

## Build Android (ARCore)
- Configurar:
  - Android Build Support + SDK/NDK.
  - XR Plug-in Management: habilitar ARCore.
  - Player Settings: min API (compatível), IL2CPP, ARM64.
  - Permissões: câmera.

## Validação/Entrega
- Testes:
  - Testar em 1–2 aparelhos ARCore.
  - Casos: tracking perdido, rotação do aparelho, iluminação fraca.
- Entregáveis:
  - Projeto Unity organizado.
  - APK debug.
  - Pequeno guia (README) com instalação, como jogar e como adicionar questões.

## Próximo Passo (quando você confirmar o plano)
- Eu crio o projeto Unity do zero nessa pasta, configuro AR Foundation/ARCore, monto a cena ARGameplay e implemento o MVP (colocação + manipulação + sistema de questões + UI + score), e deixo pronto para você compilar o APK.