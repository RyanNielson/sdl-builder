package game

import "core:fmt"
import sdl "vendor:sdl3"

main :: proc() {
	if !sdl.Init({.VIDEO}) {
		fmt.eprintln("SDL init failed:", sdl.GetError())
		return
	}
	defer sdl.Quit()

	window := sdl.CreateWindow("Foo", 100, 100, {.RESIZABLE})
	if window == nil {
		fmt.eprintln("Window creation failed:", sdl.GetError())
		return
	}
	defer sdl.DestroyWindow(window)
	sdl.ShowWindow(window)

	running := true
	for running {
		event: sdl.Event
		for sdl.PollEvent(&event) {
			#partial switch event.type {
			case .QUIT:
				running = false
			}
		}
	}
}
