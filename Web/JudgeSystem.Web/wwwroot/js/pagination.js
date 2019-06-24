﻿window.onload = () => {
	let currentPageClass = 'current-page';
	$(".page-number")[0].classList.add(currentPageClass);
};

function InitializePaginationList(url, numberOfPagesUrl) {
	let currentPageClass = 'current-page';

	$(".page-number").on("click", (e) => {
		let page = Number.parseInt(e.target.textContent);
		getHtml(url, page);
	});


	$("#previous").on('click', () => {
		let currentPageNumber = Number.parseInt($(`.${currentPageClass}`)[0].innerText);
		$.get(numberOfPagesUrl)
			.done(lastPage => {
				if (currentPageNumber === 1) {
					getHtml(url, Number.parseInt(lastPage));
				}
				else {
					getHtml(url, --currentPageNumber);
				}
			});
	});

	$("#next").on('click', () => {
		let currentPageNumber = Number.parseInt($(`.${currentPageClass}`)[0].innerText);
		$.get(numberOfPagesUrl)
			.done((lastPage) => {
				if (currentPageNumber === Number.parseInt(lastPage)) {
					getHtml(url, 1);
				}
				else {
					getHtml(url, ++currentPageNumber);
				}
			});
	});
}

function getHtml(url, page) {
	let pageNumberPlaceholder = "{pageNumber}";
	let targetUrl = url.replace(pageNumberPlaceholder, page);
	$.get(targetUrl)
		.done(html => {
			$(".container > main")[0].innerHTML = html;
			InitializePaginationList(url, numberOfPagesUrl);
			let currentPageClass = "current-page";
			$(`.page-number:contains(${page})`)[0].classList.add(currentPageClass);
		})
		.fail(error => {
			console.error(error);
		});
}