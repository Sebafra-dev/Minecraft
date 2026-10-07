# Minecraft Clone / Voxel Engine

A lightweight voxel engine inspired by Minecraft, built from scratch to explore 3D graphics, procedural generation, and memory optimization. The main focus of this project is efficient chunk rendering and low-level resource management.

---

## 🚀 Key Features

* **Procedural Terrain Generation:** Utilizes Perlin Noise (FastNoiseLite https://github.com/Auburn/FastNoiseLite/tree/master/CSharp) to generate smooth, natural-looking landscapes and heightmaps.
* **Infinite World (Chunk System):** Dynamically loads and unloads chunks based on the camera position to keep the memory footprint low.
* **Custom Architecture:** All systems are written from scratch and integrated into a clean, object-oriented codebase.

---

## 🛠️ Optimizations & Technical Challenges

### 🔧 Custom Greedy Meshing & Memory Management
To achieve high framerates and avoid performance bottlenecks, I implemented and heavily refactored a **Greedy Meshing** algorithm. 
* **Adjustment:** Modified the standard greedy mesher to fit my custom class architecture.
* **Draw Call Minimization:** Combined adjacent faces of identical block types into larger meshes, drastically cutting down the number of polygons sent to the GPU.
* **Data Packing:** Packed block data into smaller data types to optimize vertex buffer layouts to reduce RAM/VRAM.

---

## 💻 Tech Stack

* **Language:** C# (.NET)
* **Graphics API / Library:** MonoGame Framework (https://monogame.net/)
* **Development Tools:** Visual Studio

---

## 📷 Media

[![Minecraft Clone Preview](https://youtube.com)](https://www.youtube.com/watch?v=3Us9RVK_9QE)
*Click the image above to watch the gameplay and rendering engine demo on YouTube.*
