# GeoAR - Jogo de Geometria em Realidade Aumentada

## Sumário
- [Descrição](#descrição)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Requisitos do Sistema](#requisitos-do-sistema)
- [Configuração e Execução](#configuração-e-execução)
  - [Abrindo o Projeto](#abrindo-o-projeto)
  - [Configuração do ARCore](#configuração-do-arcore)
  - [Executando no Editor](#executando-no-editor)
  - [Build APK para Android](#build-apk-para-android)
- [Como Jogar](#como-jogar)
  - [Controles](#controles)
  - [Mecânicas do Jogo](#mecânicas-do-jogo)
- [Banco de Questões](#banco-de-questões)
  - [Estrutura do JSON](#estrutura-do-json)
  - [Tipos de Formas](#tipos-de-formas)
  - [Métricas Disponíveis](#métricas-disponíveis)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Licença](#licença)

## Descrição

**AR Geometry Game** é um jogo educativo de geometria desenvolvido em Unity que utiliza Realidade Aumentada (ARCore) para criar uma experiência de aprendizado imersiva. O jogo permite que os jogadores visualizem formas geométricas 2D e 3D no mundo real através da câmera do dispositivo Android e resolvam questões relacionadas a cálculos de área, perímetro, volume e área total.

A aplicação oferece uma forma interativa e inovadora de aprender conceitos geométricos, combinando visualização espacial com desafios matemáticos em um ambiente de realidade aumentada.

## Funcionalidades

- ✅ **Visualização AR**: Coloque formas geométricas no ambiente real usando ARCore
- ✅ **Detecção de Planos**: Identificação automática de superfícies planas no ambiente
- ✅ **Formas 2D e 3D**: Suporte para retângulos, triângulos, círculos, cubos, paralelepípedos, cilindros e esferas
- ✅ **Cálculos Geométricos**: Resolva questões de perímetro, área, volume e área total
- ✅ **Manipulação Interativa**: Mova, rotacione e escale formas com gestos touch
- ✅ **Sistema de Questões**: Banco de questões customizável em formato JSON
- ✅ **Feedback Visual**: Reticle de posicionamento e visualização de planos AR
- ✅ **Tela de Resultados**: Acompanhe seu desempenho após cada sessão
- ✅ **Validação de Respostas**: Sistema de tolerância numérica para verificação de respostas

## Tecnologias Utilizadas

- **Engine**: Unity 2022.3 LTS (2022.3.20f1)
- **Framework AR**: AR Foundation + ARCore
- **Plataforma**: Android (API Level 24+)
- **Linguagem**: C# (.NET Standard 2.1)
- **Scripting Backend**: IL2CPP
- **Arquitetura**: ARM64
- **Gerenciamento de Dados**: JSON (StreamingAssets)

## Requisitos do Sistema

### Desenvolvimento
- Unity 2022.3 LTS ou superior
- Android Build Support (SDK/NDK + OpenJDK)
- Unity Hub (recomendado)

### Dispositivo
- Dispositivo Android compatível com ARCore
- Android 7.0 (API Level 24) ou superior
- Arquitetura ARM64
- Câmera traseira funcional

## Configuração e Execução

### Abrindo o Projeto

1. Clone ou baixe este repositório:
   ```bash
   git clone https://github.com/wesleygatinho/cg-2026-geo.git
   cd cg-2026-geo
   ```

2. Abra o Unity Hub e clique em **Add > Add project from disk**

3. Navegue até a pasta do projeto e selecione-a

4. Aguarde a resolução de pacotes pelo Package Manager

### Configuração do ARCore

Esta configuração precisa ser feita apenas uma vez:

1. Vá em **Edit > Project Settings > XR Plug-in Management**

2. Selecione a aba **Android** e habilite **ARCore**

3. Confirme as configurações em **Project Settings > Player**:
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 (marque apenas ARM64)
   - **Minimum API Level**: Android 7.0 'Nougat' (API level 24) ou superior

4. (Opcional) Execute o validador do projeto em **Tools > AR Geometry Game > Validar Projeto**

### Executando no Editor

1. Abra a cena `Assets/Scenes/Boot.unity`

2. Clique no botão **Play** no Editor

3. O fluxo do jogo irá automaticamente:
   - Inicializar na cena Boot
   - Transicionar para o Menu
   - Permitir acesso à cena AR

> **Nota**: O conteúdo das cenas é propositalmente mínimo. O jogo instancia controladores, AR e UI automaticamente via `RuntimeInitializeOnLoad`.

### Build APK para Android

1. Acesse **File > Build Settings**

2. Selecione **Android** como plataforma e clique em **Switch Platform** (se necessário)

3. Adicione as cenas na seguinte ordem:
   1. `Assets/Scenes/Boot.unity`
   2. `Assets/Scenes/Menu.unity`
   3. `Assets/Scenes/ARGameplay.unity`
   4. `Assets/Scenes/Results.unity`

4. Configure as opções de build:
   - Development Build: opcional (para debug)
   - Compression Method: LZ4 (recomendado para desenvolvimento)

5. Clique em **Build And Run** para gerar o APK e instalar no dispositivo conectado

## Como Jogar

### Controles

#### Navegação AR
- **Mover o dispositivo**: Detecta planos no ambiente
- **Toque simples**: Coloca a forma geométrica no plano detectado
- **Botão "Mostrar/Esconder Planos"**: Alterna a visualização dos planos AR

#### Manipulação da Forma
- **1 dedo (arrastar)**: Reposiciona a forma no plano selecionado
- **2 dedos (pinça)**: Escala a forma (aumenta/diminui)
- **2 dedos (rotação)**: Rotaciona a forma em torno do eixo vertical

### Mecânicas do Jogo

1. **Início**: Na tela AR, mova o dispositivo para detectar superfícies planas

2. **Detecção**: Quando planos forem detectados, eles aparecerão com uma visualização mesh

3. **Posicionamento**: Toque em um plano para colocar a forma geométrica da questão atual

4. **Visualização**: Observe a forma em AR e analise suas dimensões

5. **Resposta**: Digite a resposta numérica no campo de entrada

6. **Validação**: Clique em **Responder** para verificar sua resposta

7. **Feedback**: Receba feedback imediato sobre acerto/erro

8. **Resultados**: Ao final, visualize seu desempenho na tela de resultados

## Banco de Questões

As questões do jogo são armazenadas no arquivo `Assets/StreamingAssets/questions.json` e podem ser facilmente customizadas.

### Estrutura do JSON

```json
{
  "questions": [
    {
      "id": "q_rect_area_01",
      "prompt": "Calcule a área do retângulo (a=0,30 m, b=0,20 m).",
      "shape": 0,
      "metric": 1,
      "a": 0.30,
      "b": 0.20,
      "tolerance": 0.001,
      "unit": "m2"
    }
  ]
}
```

#### Campos Principais

- **id**: Identificador único da questão
- **prompt**: Texto da pergunta exibido ao jogador
- **shape**: Tipo de forma geométrica (ver tabela abaixo)
- **metric**: Tipo de métrica a calcular (ver tabela abaixo)
- **a, b, c, r, h**: Parâmetros dimensionais em metros
- **tolerance**: Tolerância numérica para validação da resposta
- **unit**: Unidade de medida exibida (`"m"`, `"m2"`, `"m3"`)

### Tipos de Formas

| Valor | Forma | Parâmetros Utilizados |
|-------|-------|----------------------|
| 0 | Rectangle (Retângulo) | a, b |
| 1 | Triangle (Triângulo) | a, b, c |
| 2 | Circle (Círculo) | r |
| 3 | Cube (Cubo) | a |
| 4 | Cuboid (Paralelepípedo) | a, b, c |
| 5 | Cylinder (Cilindro) | r, h |
| 6 | Sphere (Esfera) | r |

### Métricas Disponíveis

| Valor | Métrica | Aplicável a |
|-------|---------|-------------|
| 0 | Perimeter (Perímetro) | Formas 2D |
| 1 | Area (Área) | Formas 2D |
| 2 | Volume | Formas 3D |
| 3 | SurfaceArea (Área Total) | Formas 3D |

### Adicionando Novas Questões

Para adicionar novas questões, edite o arquivo `questions.json` seguindo o formato existente:

1. Adicione um novo objeto JSON no array `questions`
2. Defina um `id` único
3. Escreva o `prompt` da questão
4. Configure os parâmetros apropriados para a forma escolhida
5. Defina uma `tolerance` adequada (geralmente 0.001 para métricas lineares/áreas)
6. Especifique a `unit` correta

## Estrutura do Projeto

```
Assets/
├── Scenes/              # Cenas do jogo
│   ├── Boot.unity       # Cena de inicialização
│   ├── Menu.unity       # Menu principal
│   ├── ARGameplay.unity # Cena de gameplay AR
│   └── Results.unity    # Tela de resultados
├── Scripts/
│   ├── AR/              # Gerenciamento de AR e interações
│   │   ├── ARAnchorPlacementManager.cs
│   │   ├── ARBootstrapper.cs
│   │   ├── ARPlacementManager.cs
│   │   ├── ARPlaneVisibilityController.cs
│   │   └── ARReticle.cs
│   ├── Core/            # Sistema central e gerenciamento de fluxo
│   ├── Geometry/        # Lógica de formas e cálculos geométricos
│   ├── Gameplay/        # Mecânicas de jogo e questões
│   └── UI/              # Interface do usuário
├── Prefabs/             # Prefabs de formas e UI
├── Resources/           # Recursos carregados em runtime
│   └── questions.json
└── StreamingAssets/     # Arquivos de dados externos
    └── questions.json   # Banco de questões
```

### Principais Componentes

- **AR**: Gerencia toda a funcionalidade de realidade aumentada, incluindo detecção de planos, posicionamento de objetos e interações touch
- **Core**: Contém a lógica central do jogo, gerenciamento de estado e transições de cena
- **Geometry**: Define as formas geométricas e implementa os cálculos matemáticos
- **Gameplay**: Controla o fluxo de gameplay, sistema de questões e validação de respostas
- **UI**: Gerencia todas as interfaces do usuário e feedback visual

## Licença

Este projeto é disponibilizado para fins educacionais.

