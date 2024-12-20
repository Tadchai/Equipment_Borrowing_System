const apiBaseUrl = 'http://localhost:5143/api/item'; // เปลี่ยน URL ตาม backend ของคุณ

// Fetch all Items
async function fetchItems() {
    const response = await fetch(apiBaseUrl);
    const Items = await response.json();
    renderItems(Items);
}

// Add new Items
document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const name = document.getElementById('ItemName').value;

    const response = await fetch(apiBaseUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
    });

    if (response.ok) {
        alert('Item added successfully');
        document.getElementById('createForm').reset();
        fetchItems();
    } else {
        alert('Error adding Item');
    }
});

// Search Items by name
document.getElementById('searchBtn').addEventListener('click', async () => {
    const name = document.getElementById('searchName').value;

    const response = await fetch(`${apiBaseUrl}/search/${name}`);
    const items = await response.json();
    renderItems(items);
});

// Render item list
function renderItems(items) {
    const list = document.getElementById('itemList');
    list.innerHTML = '';
    items.forEach((item) => {
        const row = `
            <tr>
                <td>${item.name}</td>
            </tr>
        `;
        list.insertAdjacentHTML('beforeend', row);
    });
}

fetchItems();
