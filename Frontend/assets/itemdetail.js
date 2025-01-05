const urlParams = new URLSearchParams(window.location.search);
const itemCategoryId = urlParams.get('id');

document.getElementById('updateButton').addEventListener('click', () => {
    window.location.href = `/Frontend/itemupdate.html?id=${itemCategoryId}`;
  });

async function fetchItemDetail() {
  try {
    const response = await fetch(`http://localhost:5143/Item/GetById?id=${itemCategoryId}`);
    const data = await response.json();

    if (!response.ok || !data) {
      document.getElementById('itemDetail').innerHTML = `<p>Error: ${data.message || 'Failed to load item details.'}</p>`;
      return;
    }

    let html = `<h2>Item Category</h2>`;
    html += `<p>Name: ${data.name}</p>`;

    html += `<h3>Item Classifications</h3>`;
    data.itemClassifications.forEach(classification => {
      html += `<div class="classification-box">`;
      html += `<div class="classification-title">Name: ${classification.name}</div>`;

      html += `<h4>Item Instances</h4>`;
      if (classification.itemInstances.length > 0) {
        html += `<table>
          <thead>
            <tr>
              <th>Asset ID</th>
              <th>Requisition Employee Name</th>
            </tr>
          </thead>
          <tbody>`;
        classification.itemInstances.forEach(instance => {
          html += `<tr>
            <td>${instance.assetId}</td>
            <td>${instance.requisitionEmployeeName || '-'}</td>
          </tr>`;
        });
        html += `</tbody></table>`;
      } else {
        html += `<p>No instances available.</p>`;
      }

      html += `</div>`; 
    });

    document.getElementById('itemDetail').innerHTML = html;
  } catch (error) {
    console.error('Error fetching item detail:', error);
    document.getElementById('itemDetail').innerHTML = `<p>An error occurred while fetching item details.</p>`;
  }
}

document.getElementById("deleteButton").addEventListener("click", async () => {
  const confirmation = confirm("Are you sure you want to delete this item?");
  if (!confirmation) return;

  try {
    const response = await fetch(`http://localhost:5143/Item/Delete?id=${itemCategoryId}`, {
      method: "POST",
    });

    const result = await response.json();

    if (response.ok) {
      alert(result.message);
      window.location.href = "/Frontend/item.html";
    } else {
      alert(result.message || "Failed to delete the item.");
      window.location.href = `/Frontend/itemdetail.html?id=${itemCategoryId}`;
    }
  } catch (error) {
    console.error("Error:", error);
    alert("An error occurred while deleting the item.");
  }
});

fetchItemDetail();
