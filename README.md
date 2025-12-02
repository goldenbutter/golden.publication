# Overview

Thank you for your interest in joining the engineering team at **Microchip Technology Inc.** Microchip is committed to making innovative design easier through total system solutions that address critical challenges at the intersection of emerging technologies and durable end markets. Our easy-to-use development tools and comprehensive product portfolio support customers throughout the design process, from concept to completion.

As part of a collaborative development culture, you'll work closely with engineers across disciplines to design, develop, test, deploy, and support reliable solutions for our global customers. We believe the people closest to the work are in the best position to shape how that work should be done, and that requires a commitment to engineering excellence, continuous improvement, craftsmanship, and teamwork.

This coding exercise is part of our interview process. It is not intended to trick you or to test for obscure knowledge; rather, it helps us understand how you think about design, structure your code, and solve problems. There is no single 'right' answer – we value clarity, maintainability, and your ability to communicate engineering intent.

We respect the time investment it takes to apply and to complete an exercise like this. While you should feel free to complete it in your own style, most candidates spend about **one to two hours** on the core requirements.

---

## Your Submission

Your task is to build the foundation of a **Publications Management System** using a small dataset provided in XML format.

The system revolves around *publications* and their *versions*. A publication may represent documents commonly used across Microchip's ecosystem, such as datasheets, user manuals, or reference guides.

---

## 1. Create a .NET Web API

Build a **.NET Web API** that exposes publication data as **JSON**. The XML file represents a simplified version of data you might retrieve from a product documentation or content storage system.

Your Web API should support the following features:

---

### **A. List publications (paged)**

- Return a paginated list of publications (default: **10 per page**).
- Each listed item should include:
  - `id`
  - `publication_type`
  - `title`
  - `isbn`
- Allow the client to request:
  - Custom page number  
  - Custom page size

---

### **B. Sorting**

Allow sorting by one or more of:

- `title`
- `publication_type`
- `isbn`

Sorting direction (ascending / descending) should be controlled via query parameters.

---

### **C. Searching / Filtering**

Support searching by partial:

- `title`
- `isbn`

Example query:

```
/publications?title=controller&isbn=978
```

---

### **D. Publication details with versions**

Create an endpoint that retrieves a **single publication** by its `id`.

The returned item should include **all versions**, each containing:

- `id`
- `publication_guid`
- `version` (e.g., `1`, `1.1`, `2.3`)
- `language`
- `cover_title`

---

### **E. Optional Enhancements (not required)**

Optional if you want to demonstrate additional engineering style:

- `/publications/{id}/versions` endpoint
- Global error handling
- Caching of parsed XML
- Dependency Injection for repository services
- Logging
- Additional unit tests

---

## 2. (Optional) Client-Side Application

You may also create a **client-side application** using Angular, React, Vue, or another modern framework.

If implemented, your client app may:

- Display a paginated list of publications (10 per page)
- Support sorting
- Display all publication versions
- Support search by partial `title` or `isbn`

This is optional.

---

## What's Included in This Repository

- A **dummy XML data file** (`publications.xml`)
- A basic **XML-backed repository class**
- Unit tests validating repository behavior

You may use or modify these components as you see fit.

---

## Getting Started

1. Clone this repository.
2. Create a submission branch:
   ```
   submission/<your-name-or-id>
   ```
   Examples:
   - `submission/01dec2025`
   - `submission/jane-doe`
   - `submission/publications-api`
3. Add your solution to this branch.

---

## Submitting Your Work

You may submit your work by **zipping the repository** and sending the archive or link via your HR recruiter.

Coordinate with your HR representative if another method is preferred.

---

Thank you again for your interest in **Microchip Technology Inc.**  
We look forward to reviewing your solution and discussing your approach!
