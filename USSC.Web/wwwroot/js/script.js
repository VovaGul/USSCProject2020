"use strict";document.querySelectorAll(".dropdown__current").forEach(function(e){e.addEventListener("click",function(e){e.currentTarget.nextElementSibling.classList.toggle("d-none")})});