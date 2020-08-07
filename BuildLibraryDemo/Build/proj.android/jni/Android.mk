LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)


LOCAL_MODULE := Recast

LOCAL_MODULE_FILENAME := libRecast

LOCAL_SRC_FILES := ../../src/RecastDLL.cpp

LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../src

include $(BUILD_SHARED_LIBRARY)
