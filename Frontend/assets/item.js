const apiBaseUrl = "http://localhost:5143/Item";
const itemTableBody = document.getElementById("itemTableBody");
const searchText = document.getElementById("searchText");
const searchButton = document.getElementById("searchButton");
const pageNumber = document.getElementById("pageNumber");
const pageSize = document.getElementById("pageSize");
const totalRows = document.getElementById("totalRows");

document.getElementById('addItem').addEventListener('click', () => {
    window.location.href = '/Frontend/itemcreate.html';
  });

let currentPage = 1;

async function fetchItems()
{
    const input = {
        Name: searchText.value || null,
        Page: currentPage - 1,
        PageSize: parseInt(pageSize.value),
    };

    try
    {
        const response = await fetch(`${apiBaseUrl}/Search`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(input),
        });

        if (!response.ok) throw new Error("Error fetching items");

        const data = await response.json();
        renderItems(data);
    } catch (error)
    {
        console.error(error);
        alert("Failed to fetch items");
    }
}

function renderItems(response)
{
    itemTableBody.innerHTML = "";

    response.data.forEach((item) =>
    {
        const row = document.createElement("tr");
        row.innerHTML = `
                    <td><a href="/Frontend/itemdetail.html?id=${item.itemCategoryId}">${item.name}</a></td>
                `;
        itemTableBody.appendChild(row);
    });

    updatePagination(response);
}

function updatePagination(response)
{
    totalRows.textContent = `Total Rows: ${response.rowCount}`;
    const totalPages = Math.ceil(response.rowCount / parseInt(pageSize.value));

    pageNumber.innerHTML = "";
    for (let i = 1; i <= totalPages; i++)
    {
        const option = document.createElement("option");
        option.value = i;
        option.textContent = i;
        if (i === currentPage) option.selected = true;
        pageNumber.appendChild(option);
    }
}

searchButton.addEventListener("click", fetchItems);

pageSize.addEventListener("change", () =>
{
    currentPage = 1;
    fetchItems();
});

pageNumber.addEventListener("change", () =>
{
    currentPage = parseInt(pageNumber.value);
    fetchItems();
});

window.onload = fetchItems;
