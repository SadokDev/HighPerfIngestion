HighPerfIngestion 🚀

High-Throughput Event Ingestion System — .NET 10

HighPerfIngestion is a high-performance, multi-producer event ingestion server built in .NET 10 to explore real-world concurrency, async processing, and backpressure under load.

This project focuses on how systems behave, not just how they compile.

🎯 What This Project Demonstrates

High-throughput producer/consumer architectures

Channel<T> backpressure and bounded buffering

Async vs CPU-bound workload behavior

Thread pool saturation and starvation

Fast-path vs slow-path execution

Allocation reduction using ValueTask

Throughput vs latency tradeoffs

Live metrics and performance tuning

🏗️ Architecture Overview
Producers (concurrent tasks)
        │
        ▼
Bounded Channel<Event>
(backpressure)
        │
        ▼
Async Consumers
(CPU / I/O workloads)
        │
        ▼
Live Metrics Dashboard

🧠 Key Features

Multiple concurrent producers
Fast, bursty, slow, and erratic event sources

Bounded async buffering
Natural backpressure using Channel<T>

Async consumers with mixed workloads
CPU-heavy, I/O-like, and hybrid processing

Fast path vs slow path optimization
Cache hits handled synchronously
Misses handled asynchronously using ValueTask

Thread pool behavior analysis
Observed starvation, recovery, and throughput collapse

Real-time metrics

Events/sec

Channel lag

Processing latency (min/avg/max)

📁 Structure
/Producers       → Event generators
/Processing      → Consumers & workloads
/Infrastructure  → Channels & concurrency
/Metrics         → Counters & dashboards

🧪 Experiments Performed

Channel capacity tuning (100 → 10,000)

Scaling consumer count (1 → 4)

CPU-bound vs async slow paths

Task vs ValueTask allocation comparison

Throughput and latency observation under stress

▶️ Running the Project
dotnet run


Metrics are displayed live via:

Console dashboard or

Minimal API endpoint (/metrics)

💡 Why This Matters

This project builds practical intuition for designing and tuning high-throughput backend systems such as telemetry pipelines, ingestion services, and event-driven platforms.

It reflects the same patterns and challenges found in real production systems.






