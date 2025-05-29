# Fluid Simulation

This project is a GPU-accelerated 2D fluid simulation built in Unity using a hybrid of **Smoothed Particle Hydrodynamics (SPH)** and **Eulerian grid-based** methods. 
It leverages C# scripts and HLSL compute shaders to achieve real-time, interactive fluid behavior.


## 🎮 Features

- 💧 **Hybrid Fluid Simulation**
  - **SPH** for particle-based dynamics and surface detail
  - **Eulerian grids** for stable pressure projection and boundary handling

- ⚡ **Real-Time Performance**
  - GPU-parallelized via HLSL compute shaders
  - Efficient particle neighbor search and pressure solver
  - Optimized for interactive use in Unity scenes

- 🎥 **Interactive Visualization**
  - Real-time visual feedback in Unity Editor
  - Adjustable parameters for simulation behavior
  - Supports basic UI input for experiments and tuning

## 📸 Demo

*(Insert GIF or screenshot here when I get time)*

## 🧪 How It Works

### SPH (Smoothed Particle Hydrodynamics)

- Particles represent fluid parcels
- Density and pressure calculated using smoothing kernels
- Forces: pressure, viscosity, external (e.g., gravity)

### Eulerian Grid

- Pressure projection step solved on a 2D grid to enforce incompressibility
- Combines well with SPH for stability and boundary handling

### GPU Acceleration

- All major computation steps run in parallel using HLSL compute shaders
- Optimized buffer management and thread grouping for speed

## 🛠️ Technologies

- Unity (URP compatible)
- C# (simulation logic, data orchestration)
- HLSL (compute shaders for physics)
- Unity Compute Buffers & Shader Dispatching

## 🚀 Getting Started

1. Clone the repo:

```bash
git clone https://github.com/Vengiro/Fluid-simulation.git
