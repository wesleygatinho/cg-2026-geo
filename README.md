# Jogo de Geometria em Realidade Aumentada (Unity + ARCore)

Projeto Unity para Android que usa AR Foundation (ARCore) para colocar formas no ambiente e resolver questões de geometria (área, perímetro, volume e área total).

## Requisitos
- Unity **2022.3 LTS** (o projeto está com `ProjectVersion` apontando para 2022.3.20f1).
- Android Build Support (SDK/NDK + OpenJDK).
- Dispositivo Android compatível com ARCore.

## Como Abrir
1. Abra esta pasta no Unity Hub (Add > Add project from disk).
2. Abra o projeto e aguarde a resolução de pacotes (Package Manager).

## Configuração ARCore (uma vez)
1. Vá em **Edit > Project Settings > XR Plug-in Management**.
2. Selecione **Android** e habilite **ARCore**.
3. Confirme em **Project Settings > Player**:
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64
   - **Minimum API Level**: 24+
4. (Opcional) Rode o validador em **Tools > AR Geometry Game > Validar Projeto**.

## Rodar no Editor
- Abra a cena [Boot.unity](Assets/Scenes/Boot.unity) e clique Play.
- O fluxo troca para Menu e depois para a cena AR.

Observação: o conteúdo das cenas é propositalmente mínimo; o jogo instancia controladores/AR/UI automaticamente via `RuntimeInitializeOnLoad`.

## Build APK (Android)
1. **File > Build Settings**:
   - Platform: Android (Switch Platform)
   - Adicione as cenas nesta ordem:
     1) `Assets/Scenes/Boot.unity`
     2) `Assets/Scenes/Menu.unity`
     3) `Assets/Scenes/ARGameplay.unity`
     4) `Assets/Scenes/Results.unity`
2. Clique **Build And Run**.

## Como Jogar
- Na cena AR:
  - Mova o celular para detectar planos.
  - Toque em um plano para colocar a forma.
  - Digite a resposta e clique **Responder**.
  - Use **Mostrar/Esconder Planos** para voltar a ver os planos.

Manipulação da forma:
- 1 dedo: toque e arraste para reposicionar no plano (se selecionado).
- 2 dedos: pinça para escalar e giro para rotacionar.

## Banco de Questões
- Arquivo: [questions.json](Assets/StreamingAssets/questions.json)
- Você pode adicionar novas questões seguindo o mesmo formato.
- Campos principais:
  - `shape` (0..6): Rectangle, Triangle, Circle, Cube, Cuboid, Cylinder, Sphere
  - `metric` (0..3): Perimeter, Area, Volume, SurfaceArea
  - Parâmetros: `a`, `b`, `c`, `r`, `h` (em metros)
  - `tolerance`: tolerância numérica da resposta
  - `unit`: `"m"`, `"m2"`, `"m3"` (apenas exibido)

## Estrutura (alto nível)
- AR: [Assets/Scripts/AR](Assets/Scripts/AR)
- Core/Fluxo: [Assets/Scripts/Core](Assets/Scripts/Core)
- Geometria/Questões: [Assets/Scripts/Geometry](Assets/Scripts/Geometry)
- Gameplay: [Assets/Scripts/Gameplay](Assets/Scripts/Gameplay)
- UI: [Assets/Scripts/UI](Assets/Scripts/UI)

