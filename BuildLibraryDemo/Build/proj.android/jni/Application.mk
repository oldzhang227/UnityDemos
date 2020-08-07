APP_ABI := armeabi-v7a x86 arm64-v8a
APP_STL := gnustl_static

APP_CPPFLAGS := -frtti -DCC_ENABLE_CHIPMUNK_INTEGRATION=1 -std=c++11 -fsigned-char
APP_LDFLAGS := -latomic
APP_PLATFORM := android-14

ifeq ($(NDK_DEBUG),1)
  APP_CPPFLAGS += -D_DEBUG -D_GNUC_ 
  APP_OPTIM := debug
else
  APP_CPPFLAGS += -DNDEBUG -D_GNUC_ -fvisibility=hidden
  APP_OPTIM := release
  cmd-strip = $(TOOLCHAIN_PREFIX)strip --strip-debug -x $1
endif
